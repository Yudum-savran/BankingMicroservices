namespace Account.Application.Commands;

using MediatR;

public class DepositMoneyCommand : IRequest<DepositMoneyResponse>
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class DepositMoneyResponse
{
    public bool Success { get; set; }
    public decimal NewBalance { get; set; }
    public string Message { get; set; } = string.Empty;
}
