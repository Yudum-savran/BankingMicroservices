using Transaction.Domain.Entities;

namespace Transaction.Domain.Repositories;

public interface ITransactionRepository
{
    Task<BankTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<BankTransaction>> GetByAccountIdAsync(Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<BankTransaction>> GetByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task AddAsync(BankTransaction transaction, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
