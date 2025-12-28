using AccountingSystem.Api.Data;
using AccountingSystem.Api.Dtos;
using AccountingSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AccountingSystem.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AccountingDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(AccountingDbContext db, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _db = db;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<TokenResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return BadRequest("Email, Password, CompanyName are required.");
        }

        var normalizedEmail = request.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return Conflict(new ProblemDetails
            {
                Title = "User already exists",
                Detail = $"A user with email '{normalizedEmail}' already exists. Use /auth/login instead.",
                Status = StatusCodes.Status409Conflict,
            });
        }

        if (!TryGetJwtSettings(out var issuer, out var audience, out var signingKey, out var expiresMinutes, out var jwtError))
        {
            return Problem(title: "JWT is not configured", detail: jwtError, statusCode: StatusCodes.Status500InternalServerError);
        }

        IDbContextTransaction? tx = null;
        if (_db.Database.IsRelational())
        {
            tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        }

        try
        {

            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = request.CompanyName.Trim(),
            };

            _db.Companies.Add(company);
            await _db.SaveChangesAsync(cancellationToken);

            var user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                CompanyId = company.Id,
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var details = new ValidationProblemDetails
                {
                    Title = "User creation failed",
                    Detail = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}")),
                };

                return ValidationProblem(details);
            }

            if (tx is not null)
            {
                await tx.CommitAsync(cancellationToken);
            }

            var token = CreateJwt(user, issuer, audience, signingKey, expiresMinutes);
            return Ok(token);

        }
        finally
        {
            if (tx is not null)
            {
                await tx.DisposeAsync();
            }
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and Password are required.");
        }

        if (!TryGetJwtSettings(out var issuer, out var audience, out var signingKey, out var expiresMinutes, out var jwtError))
        {
            return Problem(title: "JWT is not configured", detail: jwtError, statusCode: StatusCodes.Status500InternalServerError);
        }

        var normalizedEmail = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return Unauthorized();
        }

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
        {
            return Unauthorized();
        }

        var token = CreateJwt(user, issuer, audience, signingKey, expiresMinutes);
        return Ok(token);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);
        var companyId = User.FindFirstValue("company_id");

        return Ok(new { userId, email, companyId });
    }

    private static TokenResponse CreateJwt(ApplicationUser user, string issuer, string audience, string signingKey, int expiresMinutes)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(expiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("company_id", user.CompanyId.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new TokenResponse(token, expires);
    }

    private bool TryGetJwtSettings(
        out string issuer,
        out string audience,
        out string signingKey,
        out int expiresMinutes,
        out string? error)
    {
        issuer = _configuration["Jwt:Issuer"] ?? string.Empty;
        audience = _configuration["Jwt:Audience"] ?? string.Empty;
        signingKey = _configuration["Jwt:SigningKey"] ?? string.Empty;

        expiresMinutes = 60;
        var expiresMinutesText = _configuration["Jwt:AccessTokenMinutes"];
        if (!string.IsNullOrWhiteSpace(expiresMinutesText) && int.TryParse(expiresMinutesText, out var parsed))
        {
            expiresMinutes = parsed;
        }

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(signingKey))
        {
            error = "Missing JWT settings. Configure Jwt:Issuer, Jwt:Audience, Jwt:SigningKey (and optionally Jwt:AccessTokenMinutes). " +
                    "For local dev you can use User Secrets: " +
                    "dotnet user-secrets set \"Jwt:Issuer\" \"http://localhost\"; " +
                    "dotnet user-secrets set \"Jwt:Audience\" \"http://localhost\"; " +
                    "dotnet user-secrets set \"Jwt:SigningKey\" \"<long-random-string>\". " +
                    "Or set env vars: Jwt__Issuer, Jwt__Audience, Jwt__SigningKey.";
            return false;
        }

        error = null;
        return true;
    }
}
