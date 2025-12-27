namespace AccountingSystem.Api.Models;

public sealed class EntryLine
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public Transaction Transaction { get; set; } = null!;

    public int LineNumber { get; set; }

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public Guid? PartnerId { get; set; }

    public Partner? Partner { get; set; }

    public EntrySide Side { get; set; }

    public decimal Amount { get; set; }

    public string? Memo { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
