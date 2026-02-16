using Customer.Domain.Entities;

namespace Customer.Domain.Repositories;

public interface ICustomerRepository
{
    Task<BankCustomer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BankCustomer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<BankCustomer?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default);
    Task AddAsync(BankCustomer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankCustomer customer, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
