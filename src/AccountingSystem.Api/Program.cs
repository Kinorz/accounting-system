using AccountingSystem.Api.Data;
using AccountingSystem.Api.Security;
using AccountingSystem.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Optional: in Development, allow using deploy/.env.dev for local config.
// This is intentionally "missing-keys only" so User Secrets / env vars still win.
if (builder.Environment.IsDevelopment())
{
    DotEnv.AddMissingKeys(
        builder.Configuration,
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "deploy", ".env.dev")));
}

// Add services to the container.

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'ConnectionStrings:Default' is not configured.");
}

builder.Services.AddDbContext<AccountingDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Identity
builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AccountingDbContext>()
    .AddDefaultTokenProviders();

// Configure Authentication
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];

// If JWT settings are provided, configure JWT Bearer authentication
if (!string.IsNullOrWhiteSpace(jwtIssuer) && !string.IsNullOrWhiteSpace(jwtAudience) && !string.IsNullOrWhiteSpace(jwtSigningKey))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };
        });
}
// Otherwise, use DenyAll authentication scheme
else
{
    builder.Services
        .AddAuthentication(DenyAllAuthenticationDefaults.Scheme)
        .AddScheme<AuthenticationSchemeOptions, DenyAllAuthenticationHandler>(DenyAllAuthenticationDefaults.Scheme, _ => { });
}

// [Authorization]
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;

internal static class DotEnv
{
    public static void AddMissingKeys(ConfigurationManager configuration, string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var added = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0)
            {
                continue;
            }

            var rawKey = line[..equalsIndex].Trim();
            var rawValue = line[(equalsIndex + 1)..].Trim();

            // Allow optional surrounding quotes.
            if (rawValue.Length >= 2 && ((rawValue.StartsWith('"') && rawValue.EndsWith('"')) || (rawValue.StartsWith('\'') && rawValue.EndsWith('\''))))
            {
                rawValue = rawValue[1..^1];
            }

            // Map env var style (Foo__Bar) to configuration key style (Foo:Bar)
            var key = rawKey.Contains("__", StringComparison.Ordinal)
                ? rawKey.Replace("__", ":", StringComparison.Ordinal)
                : rawKey;

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            // Only add when missing; don't override User Secrets / env vars.
            if (configuration[key] is null)
            {
                added[key] = rawValue;
            }
        }

        if (added.Count > 0)
        {
            configuration.AddInMemoryCollection(added);
        }
    }
}
