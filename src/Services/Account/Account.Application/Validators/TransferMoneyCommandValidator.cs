using Account.Application.Commands;
using FluentValidation;

namespace Account.Application.Validators;

/// <summary>
/// Validator for TransferMoneyCommand
/// Ensures transfer amount and accounts are valid
/// </summary>
public class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty()
            .WithMessage("Source Account ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Source Account ID must be a valid GUID");

        RuleFor(x => x.TargetAccountId)
            .NotEmpty()
            .WithMessage("Target Account ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Target Account ID must be a valid GUID");

        RuleFor(x => new { x.SourceAccountId, x.TargetAccountId })
            .Must(x => x.SourceAccountId != x.TargetAccountId)
            .WithMessage("Source and target accounts must be different")
            .OverridePropertyName("TargetAccountId");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Transfer amount must be greater than 0")
            .LessThanOrEqualTo(decimal.MaxValue)
            .WithMessage("Transfer amount is too large")
            .ScalePrecision(2, 18)
            .WithMessage("Amount must have at most 2 decimal places");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");
    }
}
