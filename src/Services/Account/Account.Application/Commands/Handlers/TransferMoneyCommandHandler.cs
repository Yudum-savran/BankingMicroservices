namespace Account.Application.Commands.TransferMoney;

using Account.Application.Commands;
using Account.Application.Services;
using Account.Domain.Repositories;
using Account.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using AppInvalidOperationException = Account.Domain.Exceptions.InvalidOperationException;

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
            _logger.LogInformation(
                "Processing transfer from account {SourceAccountId} to {TargetAccountId}, Amount: {Amount}",
                request.SourceAccountId, request.TargetAccountId, request.Amount);

            var source = await _accountRepository
                .GetByIdAsync(request.SourceAccountId, cancellationToken);

            var target = await _accountRepository
                .GetByIdAsync(request.TargetAccountId, cancellationToken);

            if (source == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning("Source account {AccountId} not found for transfer operation", request.SourceAccountId);
                throw new NotFoundException("Source Account", request.SourceAccountId);
            }

            if (target == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning("Target account {AccountId} not found for transfer operation", request.TargetAccountId);
                throw new NotFoundException("Target Account", request.TargetAccountId);
            }

            source.Transfer(request.Amount, request.TargetAccountId, request.Description);
            target.Deposit(request.Amount, $"Transfer from {source.AccountNumber}");

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            foreach (var e in source.DomainEvents.Concat(target.DomainEvents))
                await _eventBus.PublishAsync(e, cancellationToken);

            source.ClearDomainEvents();
            target.ClearDomainEvents();

            _logger.LogInformation(
                "Transfer successful from {SourceAccountId} to {TargetAccountId}. Source New Balance: {Balance}",
                request.SourceAccountId, request.TargetAccountId, source.Balance.Amount);

            return new TransferMoneyResponse
            {
                Success = true,
                NewBalance = source.Balance.Amount,
                Message = "Transfer successful"
            };
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions (including InvalidOperationException from domain logic)
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        catch (NotFoundException)
        {
            // Re-throw not found exceptions
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex,
                "Unexpected error processing transfer from {SourceAccountId} to {TargetAccountId}",
                request.SourceAccountId, request.TargetAccountId);
            throw new AppInvalidOperationException("An unexpected error occurred while processing the transfer.", "TRANSFER_ERROR");
        }
    }
}

