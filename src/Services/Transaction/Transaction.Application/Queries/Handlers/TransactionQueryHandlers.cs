using MediatR;
using Transaction.Application.Queries;
using Transaction.Domain.Repositories;

namespace Transaction.Application.Queries.Handlers;

public class GetAccountTransactionsQueryHandler : IRequestHandler<GetAccountTransactionsQuery, List<TransactionDto>>
{
    private readonly ITransactionRepository _repository;

    public GetAccountTransactionsQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TransactionDto>> Handle(GetAccountTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetByAccountIdAsync(
            request.AccountId, 
            request.Page, 
            request.PageSize, 
            cancellationToken);

        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            Type = t.Type.ToString(),
            Amount = t.Amount,
            Currency = t.Currency,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            Timestamp = t.Timestamp,
            Status = t.Status.ToString(),
            RelatedAccountId = t.RelatedAccountId,
            ReferenceNumber = t.ReferenceNumber
        }).ToList();
    }
}

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto?>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionByIdQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransactionDto?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.TransactionId, cancellationToken);
        
        if (transaction == null)
            return null;

        return new TransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Type = transaction.Type.ToString(),
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            Description = transaction.Description,
            Timestamp = transaction.Timestamp,
            Status = transaction.Status.ToString(),
            RelatedAccountId = transaction.RelatedAccountId,
            ReferenceNumber = transaction.ReferenceNumber
        };
    }
}

public class GetTransactionsByDateRangeQueryHandler : IRequestHandler<GetTransactionsByDateRangeQuery, List<TransactionDto>>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionsByDateRangeQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TransactionDto>> Handle(GetTransactionsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetByDateRangeAsync(
            request.AccountId, 
            request.StartDate, 
            request.EndDate, 
            cancellationToken);

        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            Type = t.Type.ToString(),
            Amount = t.Amount,
            Currency = t.Currency,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            Timestamp = t.Timestamp,
            Status = t.Status.ToString(),
            RelatedAccountId = t.RelatedAccountId,
            ReferenceNumber = t.ReferenceNumber
        }).ToList();
    }
}
