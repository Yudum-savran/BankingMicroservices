namespace Notification.API.Events;

// Event DTOs - Centralized
public record AccountCreatedEvent(Guid AccountId, Guid CustomerId, string AccountNumber, string AccountType);
public record MoneyDepositedEvent(Guid AccountId, decimal Amount, decimal NewBalance, string Description);
public record MoneyWithdrawnEvent(Guid AccountId, decimal Amount, decimal NewBalance, string Description);
public record MoneyTransferredEvent(Guid SourceAccountId, Guid TargetAccountId, decimal Amount, decimal NewBalance, string Description);
