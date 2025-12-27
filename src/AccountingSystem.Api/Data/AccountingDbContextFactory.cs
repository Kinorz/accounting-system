using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AccountingSystem.Api.Data;

public sealed class AccountingDbContextFactory : IDesignTimeDbContextFactory<AccountingDbContext>
{
    public AccountingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:Default' is not configured.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AccountingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AccountingDbContext(optionsBuilder.Options);
    }
}
