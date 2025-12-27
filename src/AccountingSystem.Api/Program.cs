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
