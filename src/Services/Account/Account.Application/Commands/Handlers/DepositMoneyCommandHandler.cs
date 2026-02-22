namespace Account.Application.Commands.Handlers;

using Account.Application.Services;
using Account.Domain.Repositories;
using Account.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using AppInvalidOperationException = Account.Domain.Exceptions.InvalidOperationException;

public class DepositMoneyCommandHandler
    : IRequestHandler<DepositMoneyCommand, DepositMoneyResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<DepositMoneyCommandHandler> _logger;

    public DepositMoneyCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<DepositMoneyCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<DepositMoneyResponse> Handle(
        DepositMoneyCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing deposit for account {AccountId}, Amount: {Amount}", 
                request.AccountId, request.Amount);

            var account = await _accountRepository
                .GetByIdAsync(request.AccountId, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for deposit operation", request.AccountId);
                throw new NotFoundException("Account", request.AccountId);
            }

            account.Deposit(request.Amount, request.Description);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var e in account.DomainEvents)
                await _eventBus.PublishAsync(e, cancellationToken);

            account.ClearDomainEvents();

            _logger.LogInformation("Deposit successful for account {AccountId}. New Balance: {Balance}", 
                request.AccountId, account.Balance.Amount);

            return new DepositMoneyResponse
            {
                Success = true,
                NewBalance = account.Balance.Amount,
                Message = "Deposit successful"
            };
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions (including InvalidOperationException from domain logic)
            throw;
        }
        catch (NotFoundException)
        {
            // Re-throw not found exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing deposit for account {AccountId}", request.AccountId);
            throw new AppInvalidOperationException("An unexpected error occurred while processing the deposit.", "DEPOSIT_ERROR");
        }
    }
}
