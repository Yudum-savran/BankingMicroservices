namespace Transaction.Application.Events;

// Event DTOs - Tek merkezi yer
public record MoneyDepositedEvent(Guid AccountId, decimal Amount, decimal NewBalance, string Description);
public record MoneyWithdrawnEvent(Guid AccountId, decimal Amount, decimal NewBalance, string Description);
public record MoneyTransferredEvent(Guid SourceAccountId, Guid TargetAccountId, decimal Amount, decimal NewBalance, string Description);
