using Transaction.Domain.Common;

namespace Transaction.Domain.Entities;

public class BankTransaction : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public TransactionStatus Status { get; private set; }
    public Guid? RelatedAccountId { get; private set; }
    public string? ReferenceNumber { get; private set; }

    private BankTransaction() { }

    public static BankTransaction CreateDeposit(
        Guid accountId,
        decimal amount,
        string currency,
        decimal balanceBefore,
        decimal balanceAfter,
        string description)
    {
        return new BankTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = TransactionType.Deposit,
            Amount = amount,
            Currency = currency,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Description = description,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed,
            ReferenceNumber = GenerateReferenceNumber()
        };
    }

    public static BankTransaction CreateWithdrawal(
        Guid accountId,
        decimal amount,
        string currency,
        decimal balanceBefore,
        decimal balanceAfter,
        string description)
    {
        return new BankTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = TransactionType.Withdrawal,
            Amount = amount,
            Currency = currency,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Description = description,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed,
            ReferenceNumber = GenerateReferenceNumber()
        };
    }

    public static BankTransaction CreateTransfer(
        Guid sourceAccountId,
        Guid targetAccountId,
        decimal amount,
        string currency,
        decimal balanceBefore,
        decimal balanceAfter,
        string description,
        bool isOutgoing)
    {
        return new BankTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = sourceAccountId,
            Type = isOutgoing ? TransactionType.TransferOut : TransactionType.TransferIn,
            Amount = amount,
            Currency = currency,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Description = description,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed,
            RelatedAccountId = targetAccountId,
            ReferenceNumber = GenerateReferenceNumber()
        };
    }

    private static string GenerateReferenceNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"TXN{timestamp}{random}";
    }
}

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    TransferOut = 3,
    TransferIn = 4
}

public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Reversed = 4
}
