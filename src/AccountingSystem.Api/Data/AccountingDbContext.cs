using AccountingSystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Api.Data;

public sealed class AccountingDbContext : IdentityDbContext<ApplicationUser>
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<ApplicationUser> AppUsers => Set<ApplicationUser>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<EntryLine> EntryLines => Set<EntryLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamptz");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(x => x.Id).HasMaxLength(450);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamptz");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CompanyId);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamptz");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Accounts)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ParentAccount)
                .WithMany(x => x.ChildAccounts)
                .HasForeignKey(x => x.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("partners", t =>
            {
                t.HasCheckConstraint("CK_partners_partner_type", "\"PartnerType\" in (1, 2)");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PartnerType).HasConversion<short>();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamptz");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Partners)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.CompanyId, x.Name });
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransactionDate).HasColumnType("date");
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamptz");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedTransactions)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.CompanyId, x.TransactionDate });
        });

        modelBuilder.Entity<EntryLine>(entity =>
        {
            entity.ToTable("entry_lines", t =>
            {
                t.HasCheckConstraint("CK_entry_lines_side", "\"Side\" in (1, 2)");
                t.HasCheckConstraint("CK_entry_lines_amount_positive", "\"Amount\" > 0");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Side).HasConversion<short>();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Memo).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamptz");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Transaction)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Account)
                .WithMany(x => x.EntryLines)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Partner)
                .WithMany(x => x.EntryLines)
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.TransactionId, x.LineNumber }).IsUnique();
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTimestamps()
    {
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            if (entry.Metadata.FindProperty(nameof(Company.CreatedAt)) is not null && entry.State == EntityState.Added)
            {
                entry.Property(nameof(Company.CreatedAt)).CurrentValue = utcNow;
            }

            if (entry.Metadata.FindProperty(nameof(Company.UpdatedAt)) is not null)
            {
                entry.Property(nameof(Company.UpdatedAt)).CurrentValue = utcNow;
            }
        }
    }
}
