using Microsoft.AspNetCore.Identity;

namespace AccountingSystem.Api.Models;

public sealed class ApplicationUser : IdentityUser
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Transaction> CreatedTransactions { get; set; } = new List<Transaction>();
}
