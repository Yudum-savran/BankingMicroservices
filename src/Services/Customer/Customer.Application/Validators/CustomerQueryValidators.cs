using Customer.Application.Queries;
using FluentValidation;

namespace Customer.Application.Validators;

/// <summary>
/// Validator for GetCustomerByIdQuery
/// </summary>
public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Customer ID must be a valid GUID");
    }
}
