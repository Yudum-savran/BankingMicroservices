using Transaction.Application.Queries;
using FluentValidation;

namespace Transaction.Application.Validators;

/// <summary>
/// Validator for GetAccountTransactionsQuery
/// Ensures transaction query parameters are valid
/// </summary>
public class GetAccountTransactionsQueryValidator : AbstractValidator<GetAccountTransactionsQuery>
{
    public GetAccountTransactionsQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Account ID must be a valid GUID");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100");
    }
}

/// <summary>
/// Validator for GetTransactionByIdQuery
/// Ensures transaction ID is valid
/// </summary>
public class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("Transaction ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Transaction ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for GetTransactionsByDateRangeQuery
/// Ensures date range parameters are valid
/// </summary>
public class GetTransactionsByDateRangeQueryValidator : AbstractValidator<GetTransactionsByDateRangeQuery>
{
    public GetTransactionsByDateRangeQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Account ID must be a valid GUID");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");
    }
}
