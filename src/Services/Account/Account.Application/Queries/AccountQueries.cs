using MediatR;

namespace Account.Application.Queries;

/// <summary>
/// CQRS Query - Read side
/// Queries are for reading data without side effects
/// Can be optimized separately from write side
/// </summary>
public class GetAccountByIdQuery : IRequest<AccountDto?>
{
    public Guid AccountId { get; set; }

    public GetAccountByIdQuery(Guid accountId)
    {
        AccountId = accountId;
    }
}

public class GetAccountsByCustomerIdQuery : IRequest<List<AccountDto>>
{
    public Guid CustomerId { get; set; }

    public GetAccountsByCustomerIdQuery(Guid customerId)
    {
        CustomerId = customerId;
    }
}

public class GetAccountByNumberQuery : IRequest<AccountDto?>
{
    public string AccountNumber { get; set; }

    public GetAccountByNumberQuery(string accountNumber)
    {
        AccountNumber = accountNumber;
    }
}

/// <summary>
/// DTO for read operations - optimized for queries
/// Different from domain entities
/// </summary>
public class AccountDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public decimal DailyWithdrawLimit { get; set; }
    public decimal DailyWithdrawnAmount { get; set; }
}
