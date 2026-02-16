using Account.Domain.Entities;
using MediatR;

namespace Account.Application.Commands;

/// <summary>
/// CQRS Command - Write side
/// Commands represent user intentions to change the system state
/// </summary>
public class CreateAccountCommand : IRequest<CreateAccountResponse>
{
    public Guid CustomerId { get; set; }
    public AccountType AccountType { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal DailyWithdrawLimit { get; set; } = 10000;
}

public class CreateAccountResponse
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
