using MediatR;

namespace Transaction.Application.Queries;

public class GetAccountTransactionsQuery : IRequest<List<TransactionDto>>
{
    public Guid AccountId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetTransactionByIdQuery : IRequest<TransactionDto?>
{
    public Guid TransactionId { get; set; }
}

public class GetTransactionsByDateRangeQuery : IRequest<List<TransactionDto>>
{
    public Guid AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? RelatedAccountId { get; set; }
    public string? ReferenceNumber { get; set; }
}
