using Account.Domain.Entities;
using Account.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Account.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation - Infrastructure layer
/// Implements domain repository interface using EF Core
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly AccountDbContext _context;

    public AccountRepository(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<BankAccount?> GetByAccountNumberAsync(
        string accountNumber, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task<List<BankAccount>> GetByCustomerIdAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<BankAccount> AddAsync(
        BankAccount account, 
        CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
        return account;
    }

    public Task UpdateAsync(BankAccount account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == id, cancellationToken);
    }
}

/// <summary>
/// Unit of Work implementation
/// Manages database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AccountDbContext _context;

    public UnitOfWork(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }
}
