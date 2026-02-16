using Microsoft.EntityFrameworkCore;
using Transaction.Domain.Entities;
using Transaction.Domain.Repositories;
using Transaction.Infrastructure.Persistence;

namespace Transaction.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _context;

    public TransactionRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<BankTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<BankTransaction>> GetByAccountIdAsync(Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BankTransaction>> GetByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && t.Timestamp >= startDate && t.Timestamp <= endDate)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly TransactionDbContext _context;

    public UnitOfWork(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
