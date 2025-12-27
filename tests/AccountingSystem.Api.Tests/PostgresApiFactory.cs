using AccountingSystem.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AccountingSystem.Api.Tests;

public sealed class PostgresApiFactory : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;
    private string? _previousEnvironment;
    private string? _previousConnectionString;
    private string? _previousJwtIssuer;
    private string? _previousJwtAudience;
    private string? _previousJwtSigningKey;
    private string? _previousJwtAccessTokenMinutes;

    public WebApplicationFactory<Program> Factory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var dbName = "accounting_test";
        var user = "accounting_test";
        var password = "accounting_test";

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase(dbName)
            .WithUsername(user)
            .WithPassword(password)
            .Build();

        await _postgres.StartAsync();

        var connectionString = _postgres.GetConnectionString();

        _previousEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        _previousJwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
        _previousJwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience");
        _previousJwtSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        _previousJwtAccessTokenMinutes = Environment.GetEnvironmentVariable("Jwt__AccessTokenMinutes");

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", connectionString);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "http://localhost");
        Environment.SetEnvironmentVariable("Jwt__Audience", "http://localhost");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "test-signing-key-change-me-to-a-longer-random-string");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", "60");

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _previousEnvironment);
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _previousConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Issuer", _previousJwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", _previousJwtAudience);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", _previousJwtSigningKey);
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", _previousJwtAccessTokenMinutes);

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }
}
