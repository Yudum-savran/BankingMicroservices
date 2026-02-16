using Account.Domain.Common;
using Account.Domain.Events;
using Account.Domain.ValueObjects;

namespace Account.Domain.Entities;

/// <summary>
/// Account Aggregate Root - DDD Pattern
/// </summary>
public class BankAccount : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public string AccountNumber { get; private set; }
    public AccountType AccountType { get; private set; }
    public Money Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastTransactionDate { get; private set; }
    public string Currency { get; private set; }
    public decimal DailyWithdrawLimit { get; private set; }
    public decimal DailyWithdrawnAmount { get; private set; }
    public DateTime LastResetDate { get; private set; }

    // EF Core için private constructor
    private BankAccount() { }

    // Factory Method - Domain logic encapsulation
    public static BankAccount Create(
        Guid customerId,
        AccountType accountType,
        string currency,
        decimal dailyWithdrawLimit)
    {
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            AccountNumber = GenerateAccountNumber(),
            AccountType = accountType,
            Balance = new Money(0, currency),
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Currency = currency,
            DailyWithdrawLimit = dailyWithdrawLimit,
            DailyWithdrawnAmount = 0,
            LastResetDate = DateTime.UtcNow.Date
        };

        // Domain Event
        account.AddDomainEvent(new AccountCreatedEvent(
            account.Id,
            account.CustomerId,
            account.AccountNumber,
            account.AccountType));

        return account;
    }

    public void Deposit(decimal amount, string description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Deposit amount must be positive");

        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Account is not active");

        Balance = Balance.Add(amount);
        LastTransactionDate = DateTime.UtcNow;

        AddDomainEvent(new MoneyDepositedEvent(
            Id,
            amount,
            Balance.Amount,
            description));
    }

    public void Withdraw(decimal amount, string description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Withdrawal amount must be positive");

        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Account is not active");

        if (Balance.Amount < amount)
            throw new InvalidOperationException("Insufficient balance");

        // Günlük limit kontrolü
        ResetDailyLimitIfNeeded();

        if (DailyWithdrawnAmount + amount > DailyWithdrawLimit)
            throw new InvalidOperationException("Daily withdrawal limit exceeded");

        Balance = Balance.Subtract(amount);
        DailyWithdrawnAmount += amount;
        LastTransactionDate = DateTime.UtcNow;

        AddDomainEvent(new MoneyWithdrawnEvent(
            Id,
            amount,
            Balance.Amount,
            description));
    }

    public void Transfer(decimal amount, Guid targetAccountId, string description)
    {
        Withdraw(amount, description);

        AddDomainEvent(new MoneyTransferredEvent(
            Id,
            targetAccountId,
            amount,
            Balance.Amount,
            description));
    }

    public void Block(string reason)
    {
        if (Status == AccountStatus.Blocked)
            throw new InvalidOperationException("Account is already blocked");

        Status = AccountStatus.Blocked;

        AddDomainEvent(new AccountBlockedEvent(Id, reason));
    }

    public void Unblock()
    {
        if (Status != AccountStatus.Blocked)
            throw new InvalidOperationException("Account is not blocked");

        Status = AccountStatus.Active;

        AddDomainEvent(new AccountUnblockedEvent(Id));
    }

    public void Close()
    {
        if (Balance.Amount != 0)
            throw new InvalidOperationException("Cannot close account with non-zero balance");

        Status = AccountStatus.Closed;

        AddDomainEvent(new AccountClosedEvent(Id));
    }

    private void ResetDailyLimitIfNeeded()
    {
        var today = DateTime.UtcNow.Date;
        if (LastResetDate < today)
        {
            DailyWithdrawnAmount = 0;
            LastResetDate = today;
        }
    }

    private static string GenerateAccountNumber()
    {
        // TR + 2 kontrol hanesi + 5 hane banka kodu + 1 hane rezerve + 16 hane hesap numarası
        var random = new Random();
        var bankCode = "00001"; // Örnek banka kodu
        var accountPart = random.Next(100000000, 999999999).ToString("D16");
        return $"TR{random.Next(10, 99)}{bankCode}0{accountPart}";
    }
}

public enum AccountType
{
    Checking = 1,
    Savings = 2,
    Business = 3
}

public enum AccountStatus
{
    Active = 1,
    Blocked = 2,
    Closed = 3
}
