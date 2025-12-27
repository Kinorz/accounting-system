namespace AccountingSystem.Api.Models;

public sealed class Company
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    public ICollection<Partner> Partners { get; set; } = new List<Partner>();

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
