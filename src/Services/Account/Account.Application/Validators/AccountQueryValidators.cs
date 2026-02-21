using Account.Application.Queries;
using FluentValidation;

namespace Account.Application.Validators;

/// <summary>
/// Validator for GetAccountByIdQuery
/// </summary>
public class GetAccountByIdQueryValidator : AbstractValidator<GetAccountByIdQuery>
{
    public GetAccountByIdQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Account ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for GetAccountsByCustomerIdQuery
/// </summary>
public class GetAccountsByCustomerIdQueryValidator : AbstractValidator<GetAccountsByCustomerIdQuery>
{
    public GetAccountsByCustomerIdQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Customer ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for GetAccountByNumberQuery
/// </summary>
public class GetAccountByNumberQueryValidator : AbstractValidator<GetAccountByNumberQuery>
{
    public GetAccountByNumberQueryValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account number is required")
            .Matches(@"^TR\d{2}\d{5}0\d{16}$")
            .WithMessage("Account number must be in valid Turkish format (TR + 26 digits)");
    }
}
