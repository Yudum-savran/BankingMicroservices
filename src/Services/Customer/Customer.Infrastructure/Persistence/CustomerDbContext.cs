using Microsoft.EntityFrameworkCore;
using Customer.Domain.Entities;

namespace Customer.Infrastructure.Persistence;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<BankCustomer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankCustomer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.IdentityNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.KYCLevel).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("State").HasMaxLength(100);
                address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100);
            });

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IdentityNumber).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
