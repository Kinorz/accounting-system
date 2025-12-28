namespace AccountingSystem.Api.Models;

public sealed class Transaction
{
    public long Id { get; set; }

    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public DateOnly TransactionDate { get; set; }

    public string? Description { get; set; }

    public string? CreatedByUserId { get; set; }

    public ApplicationUser? CreatedByUser { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<EntryLine> Lines { get; set; } = new List<EntryLine>();
}
