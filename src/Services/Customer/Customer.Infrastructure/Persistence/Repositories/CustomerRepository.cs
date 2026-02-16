using Microsoft.EntityFrameworkCore;
using Customer.Domain.Entities;
using Customer.Domain.Repositories;
using Customer.Infrastructure.Persistence;

namespace Customer.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<BankCustomer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<BankCustomer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<BankCustomer?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.IdentityNumber == identityNumber, cancellationToken);
    }

    public async Task AddAsync(BankCustomer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public Task UpdateAsync(BankCustomer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(customer);
        return Task.CompletedTask;
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly CustomerDbContext _context;

    public UnitOfWork(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
