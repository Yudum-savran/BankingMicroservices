namespace Account.Application.Commands;

using MediatR;

public class TransferMoneyCommand : IRequest<TransferMoneyResponse>
{
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TransferMoneyResponse
{
    public bool Success { get; set; }
    public decimal NewBalance { get; set; }
    public string Message { get; set; } = string.Empty;
}

