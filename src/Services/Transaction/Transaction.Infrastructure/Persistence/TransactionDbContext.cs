using Microsoft.EntityFrameworkCore;
using Transaction.Domain.Entities;

namespace Transaction.Infrastructure.Persistence;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    public DbSet<BankTransaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.ToTable("Transactions");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.BalanceBefore).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            
            // Indexes
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ReferenceNumber).IsUnique();
            entity.HasIndex(e => new { e.AccountId, e.Timestamp });
        });

        base.OnModelCreating(modelBuilder);
    }
}
