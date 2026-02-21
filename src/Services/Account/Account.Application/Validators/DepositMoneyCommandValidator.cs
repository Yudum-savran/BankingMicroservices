using Account.Application.Commands;
using FluentValidation;

namespace Account.Application.Validators;

/// <summary>
/// Validator for DepositMoneyCommand
/// Ensures deposit amount is valid
/// </summary>
public class DepositMoneyCommandValidator : AbstractValidator<DepositMoneyCommand>
{
    public DepositMoneyCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Account ID must be a valid GUID");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Deposit amount must be greater than 0")
            .LessThanOrEqualTo(decimal.MaxValue)
            .WithMessage("Deposit amount is too large")
            .ScalePrecision(2, 18)
            .WithMessage("Amount must have at most 2 decimal places");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");
    }
}
