using Account.Application.Services;
using Account.Domain.Entities;
using Account.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Account.Application.Commands.Handlers;

/// <summary>
/// CQRS Command Handler - Processes write operations
/// Orchestrates domain logic and infrastructure
/// </summary>
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CreateAccountResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<CreateAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<CreateAccountResponse> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating account for customer {CustomerId}", request.CustomerId);

            // Domain logic - using factory method
            var account = BankAccount.Create(
                request.CustomerId,
                request.AccountType,
                request.Currency,
                request.DailyWithdrawLimit);

            // Persist to database
            await _accountRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publish domain events to RabbitMQ
            foreach (var domainEvent in account.DomainEvents)
            {
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
            }

            account.ClearDomainEvents();

            _logger.LogInformation(
                "Account created successfully. AccountId: {AccountId}, AccountNumber: {AccountNumber}",
                account.Id,
                account.AccountNumber);

            return new CreateAccountResponse
            {
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                Success = true,
                Message = "Account created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account for customer {CustomerId}", request.CustomerId);
            
            return new CreateAccountResponse
            {
                Success = false,
                Message = $"Error creating account: {ex.Message}"
            };
        }
    }
}
