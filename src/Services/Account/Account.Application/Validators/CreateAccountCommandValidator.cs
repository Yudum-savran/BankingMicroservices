using Account.Application.Commands;
using FluentValidation;

namespace Account.Application.Validators;

/// <summary>
/// Validator for CreateAccountCommand
/// Ensures input data meets business requirements
/// </summary>
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Customer ID must be a valid GUID");

        RuleFor(x => x.AccountType)
            .IsInEnum()
            .WithMessage("Account type must be a valid type (Checking=1, Savings=2, Business=3)");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3, 3)
            .WithMessage("Currency must be a 3-letter code (e.g., TRY, USD)")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be uppercase 3-letter code");

        RuleFor(x => x.DailyWithdrawLimit)
            .GreaterThan(0)
            .WithMessage("Daily withdraw limit must be greater than 0")
            .LessThanOrEqualTo(decimal.MaxValue)
            .WithMessage("Daily withdraw limit is too large");
    }
}
