using Account.Domain.Common;
using Account.Domain.Entities;

namespace Account.Domain.Events;

public class AccountCreatedEvent : DomainEvent
{
    public Guid AccountId { get; }
    public Guid CustomerId { get; }
    public string AccountNumber { get; }
    public AccountType AccountType { get; }

    public AccountCreatedEvent(Guid accountId, Guid customerId, string accountNumber, AccountType accountType)
    {
        AccountId = accountId;
        CustomerId = customerId;
        AccountNumber = accountNumber;
        AccountType = accountType;
    }
}

public class MoneyDepositedEvent : DomainEvent
{
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public decimal NewBalance { get; }
    public string Description { get; }

    public MoneyDepositedEvent(Guid accountId, decimal amount, decimal newBalance, string description)
    {
        AccountId = accountId;
        Amount = amount;
        NewBalance = newBalance;
        Description = description;
    }
}

public class MoneyWithdrawnEvent : DomainEvent
{
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public decimal NewBalance { get; }
    public string Description { get; }

    public MoneyWithdrawnEvent(Guid accountId, decimal amount, decimal newBalance, string description)
    {
        AccountId = accountId;
        Amount = amount;
        NewBalance = newBalance;
        Description = description;
    }
}

public class MoneyTransferredEvent : DomainEvent
{
    public Guid SourceAccountId { get; }
    public Guid TargetAccountId { get; }
    public decimal Amount { get; }
    public decimal NewBalance { get; }
    public string Description { get; }

    public MoneyTransferredEvent(
        Guid sourceAccountId,
        Guid targetAccountId,
        decimal amount,
        decimal newBalance,
        string description)
    {
        SourceAccountId = sourceAccountId;
        TargetAccountId = targetAccountId;
        Amount = amount;
        NewBalance = newBalance;
        Description = description;
    }
}

public class AccountBlockedEvent : DomainEvent
{
    public Guid AccountId { get; }
    public string Reason { get; }

    public AccountBlockedEvent(Guid accountId, string reason)
    {
        AccountId = accountId;
        Reason = reason;
    }
}

public class AccountUnblockedEvent : DomainEvent
{
    public Guid AccountId { get; }

    public AccountUnblockedEvent(Guid accountId)
    {
        AccountId = accountId;
    }
}

public class AccountClosedEvent : DomainEvent
{
    public Guid AccountId { get; }

    public AccountClosedEvent(Guid accountId)
    {
        AccountId = accountId;
    }
}
