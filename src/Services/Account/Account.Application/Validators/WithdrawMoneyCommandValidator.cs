using Account.Application.Commands;
using FluentValidation;

namespace Account.Application.Validators;

/// <summary>
/// Validator for WithdrawMoneyCommand
/// Ensures withdrawal amount is valid
/// </summary>
public class WithdrawMoneyCommandValidator : AbstractValidator<WithdrawMoneyCommand>
{
    public WithdrawMoneyCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Account ID must be a valid GUID");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Withdrawal amount must be greater than 0")
            .LessThanOrEqualTo(decimal.MaxValue)
            .WithMessage("Withdrawal amount is too large")
            .ScalePrecision(2, 18)
            .WithMessage("Amount must have at most 2 decimal places");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");
    }
}
