namespace Account.Application.Commands.Handlers;

using Account.Application.Services;
using Account.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

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
        var account = await _accountRepository
            .GetByIdAsync(request.AccountId, cancellationToken);

        if (account == null)
        {
            return new DepositMoneyResponse
            {
                Success = false,
                Message = "Account not found"
            };
        }

        account.Deposit(request.Amount, request.Description);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var e in account.DomainEvents)
            await _eventBus.PublishAsync(e, cancellationToken);

        account.ClearDomainEvents();

        return new DepositMoneyResponse
        {
            Success = true,
            NewBalance = account.Balance.Amount,
            Message = "Deposit successful"
        };
    }
}
