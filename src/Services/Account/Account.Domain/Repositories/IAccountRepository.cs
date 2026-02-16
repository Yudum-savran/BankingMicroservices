using Account.Domain.Entities;

namespace Account.Domain.Repositories;

/// <summary>
/// Repository interface - defined in Domain layer but implemented in Infrastructure
/// DDD Pattern: Domain defines contracts, infrastructure implements them
/// </summary>
public interface IAccountRepository
{
    Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<List<BankAccount>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<BankAccount> AddAsync(BankAccount account, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankAccount account, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
