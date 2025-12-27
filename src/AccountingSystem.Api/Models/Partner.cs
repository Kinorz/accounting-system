namespace AccountingSystem.Api.Models;

public sealed class Partner
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public PartnerType PartnerType { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<EntryLine> EntryLines { get; set; } = new List<EntryLine>();
}
