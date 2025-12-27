namespace AccountingSystem.Api.Models;

public sealed class Account
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid? ParentAccountId { get; set; }

    public Account? ParentAccount { get; set; }

    public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<EntryLine> EntryLines { get; set; } = new List<EntryLine>();
}
