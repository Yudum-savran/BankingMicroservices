using Microsoft.Extensions.Logging;
using Transaction.Domain.Entities;
using Transaction.Domain.Repositories;
using Transaction.Application.Events;

namespace Transaction.Application.EventHandlers;

public class AccountEventHandler
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountEventHandler> _logger;

    public AccountEventHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<AccountEventHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleMoneyDepositedEvent(MoneyDepositedEvent @event)
    {
        try
        {
            _logger.LogInformation("Processing MoneyDepositedEvent for Account {AccountId}, Amount: {Amount}", 
                @event.AccountId, @event.Amount);

            var transaction = BankTransaction.CreateDeposit(
                accountId: @event.AccountId,
                amount: @event.Amount,
                currency: "TRY",
                balanceBefore: @event.NewBalance - @event.Amount,
                balanceAfter: @event.NewBalance,
                description: @event.Description
            );

            await _repository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Transaction recorded: {ReferenceNumber}", transaction.ReferenceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MoneyDepositedEvent");
            throw;
        }
    }

    public async Task HandleMoneyWithdrawnEvent(MoneyWithdrawnEvent @event)
    {
        try
        {
            _logger.LogInformation("Processing MoneyWithdrawnEvent for Account {AccountId}, Amount: {Amount}", 
                @event.AccountId, @event.Amount);

            var transaction = BankTransaction.CreateWithdrawal(
                accountId: @event.AccountId,
                amount: @event.Amount,
                currency: "TRY",
                balanceBefore: @event.NewBalance + @event.Amount,
                balanceAfter: @event.NewBalance,
                description: @event.Description
            );

            await _repository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Transaction recorded: {ReferenceNumber}", transaction.ReferenceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MoneyWithdrawnEvent");
            throw;
        }
    }

    public async Task HandleMoneyTransferredEvent(MoneyTransferredEvent @event)
    {
        try
        {
            _logger.LogInformation("Processing MoneyTransferredEvent from {SourceAccountId} to {TargetAccountId}",
                @event.SourceAccountId, @event.TargetAccountId);

            var outgoingTransaction = BankTransaction.CreateTransfer(
                sourceAccountId: @event.SourceAccountId,
                targetAccountId: @event.TargetAccountId,
                amount: @event.Amount,
                currency: "TRY",
                balanceBefore: @event.NewBalance + @event.Amount,
                balanceAfter: @event.NewBalance,
                description: @event.Description,
                isOutgoing: true
            );

            await _repository.AddAsync(outgoingTransaction);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Transfer transaction recorded: {ReferenceNumber}", outgoingTransaction.ReferenceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MoneyTransferredEvent");
            throw;
        }
    }
}
