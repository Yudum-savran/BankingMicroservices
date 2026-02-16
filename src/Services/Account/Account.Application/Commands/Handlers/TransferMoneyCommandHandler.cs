namespace Account.Application.Commands.TransferMoney;

using Account.Application.Commands;
using Account.Application.Services;
using Account.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

public class TransferMoneyCommandHandler
    : IRequestHandler<TransferMoneyCommand, TransferMoneyResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<TransferMoneyCommandHandler> _logger;

    public TransferMoneyCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<TransferMoneyCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<TransferMoneyResponse> Handle(
        TransferMoneyCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var source = await _accountRepository
                .GetByIdAsync(request.SourceAccountId, cancellationToken);

            var target = await _accountRepository
                .GetByIdAsync(request.TargetAccountId, cancellationToken);

            if (source == null || target == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return new TransferMoneyResponse
                {
                    Success = false,
                    Message = "Source or target account not found"
                };
            }

            source.Transfer(request.Amount, request.TargetAccountId, request.Description);
            target.Deposit(request.Amount, $"Transfer from {source.AccountNumber}");

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            foreach (var e in source.DomainEvents.Concat(target.DomainEvents))
                await _eventBus.PublishAsync(e, cancellationToken);

            source.ClearDomainEvents();
            target.ClearDomainEvents();

            return new TransferMoneyResponse
            {
                Success = true,
                NewBalance = source.Balance.Amount,
                Message = "Transfer successful"
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

