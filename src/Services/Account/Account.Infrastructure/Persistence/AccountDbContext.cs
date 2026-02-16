using Account.Domain.Entities;
using Account.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Account.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext - Infrastructure layer
/// Configures database schema and entity mappings
/// </summary>
public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<BankAccount> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BankAccount entity
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("Accounts");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedNever(); // We generate Guid in domain

            entity.Property(e => e.CustomerId)
                .IsRequired();

            entity.Property(e => e.AccountNumber)
                .IsRequired()
                .HasMaxLength(26); // IBAN format

            entity.HasIndex(e => e.AccountNumber)
                .IsUnique();

            entity.Property(e => e.AccountType)
                .IsRequired()
                .HasConversion<string>(); // Store as string in DB

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.DailyWithdrawLimit)
                .HasPrecision(18, 2);

            entity.Property(e => e.DailyWithdrawnAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.LastResetDate)
                .IsRequired();

            // Configure Money Value Object (owned entity)
            entity.OwnsOne(e => e.Balance, balance =>
            {
                balance.Property(m => m.Amount)
                    .HasColumnName("Balance")
                    .HasPrecision(18, 2)
                    .IsRequired();

                balance.Property(m => m.Currency)
                    .HasColumnName("BalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Ignore domain events (they're not persisted)
            entity.Ignore(e => e.DomainEvents);

            // Indexes for performance
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
