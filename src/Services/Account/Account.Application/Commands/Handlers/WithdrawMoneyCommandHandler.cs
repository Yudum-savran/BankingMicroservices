namespace Account.Application.Commands.WithdrawMoney;

using Account.Application.Commands;
using Account.Application.Services;
using Account.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

public class WithdrawMoneyCommandHandler
    : IRequestHandler<WithdrawMoneyCommand, WithdrawMoneyResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<WithdrawMoneyCommandHandler> _logger;

    public WithdrawMoneyCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<WithdrawMoneyCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<WithdrawMoneyResponse> Handle(
        WithdrawMoneyCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository
            .GetByIdAsync(request.AccountId, cancellationToken);

        if (account == null)
        {
            return new WithdrawMoneyResponse
            {
                Success = false,
                Message = "Account not found"
            };
        }

        account.Withdraw(request.Amount, request.Description);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var e in account.DomainEvents)
            await _eventBus.PublishAsync(e, cancellationToken);

        account.ClearDomainEvents();

        return new WithdrawMoneyResponse
        {
            Success = true,
            NewBalance = account.Balance.Amount,
            Message = "Withdrawal successful"
        };
    }
}
