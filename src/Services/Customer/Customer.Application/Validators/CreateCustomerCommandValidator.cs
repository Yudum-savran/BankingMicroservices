using Customer.Application.Commands;
using FluentValidation;

namespace Customer.Application.Validators;

/// <summary>
/// Validator for CreateCustomerCommand
/// Ensures customer creation data meets business requirements
/// </summary>
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .Length(2, 100)
            .WithMessage("First name must be between 2 and 100 characters")
            .Matches(@"^[a-zA-Zç??ö?üÇ??Ö?Ü\s-]+$")
            .WithMessage("First name can only contain letters, spaces, and hyphens");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .Length(2, 100)
            .WithMessage("Last name must be between 2 and 100 characters")
            .Matches(@"^[a-zA-Zç??ö?üÇ??Ö?Ü\s-]+$")
            .WithMessage("Last name can only contain letters, spaces, and hyphens");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^(?:\+?90|0)?[1-9]\d{9}$")
            .WithMessage("Phone number must be a valid Turkish phone number");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty()
            .WithMessage("Identity number is required")
            .Length(11, 11)
            .WithMessage("Identity number must be exactly 11 digits")
            .Matches(@"^\d{11}$")
            .WithMessage("Identity number must contain only digits");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .Must(dob => CalculateAge(dob) >= 18)
            .WithMessage("Customer must be at least 18 years old");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required")
            .MaximumLength(200)
            .WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(100)
            .WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required")
            .Matches(@"^\d{5}$")
            .WithMessage("Postal code must be 5 digits");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters");
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}

/// <summary>
/// Validator for UpdateCustomerContactCommand
/// Ensures contact information updates are valid
/// </summary>
public class UpdateCustomerContactCommandValidator : AbstractValidator<UpdateCustomerContactCommand>
{
    public UpdateCustomerContactCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Customer ID must be a valid GUID");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(?:\+?90|0)?[1-9]\d{9}$")
            .WithMessage("Phone number must be a valid Turkish phone number")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

/// <summary>
/// Validator for UpgradeKYCLevelCommand
/// Ensures KYC level upgrade request is valid
/// </summary>
public class UpgradeKYCLevelCommandValidator : AbstractValidator<UpgradeKYCLevelCommand>
{
    public UpgradeKYCLevelCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Customer ID must be a valid GUID");

        RuleFor(x => x.NewLevel)
            .GreaterThan(0)
            .WithMessage("KYC level must be greater than 0")
            .LessThanOrEqualTo(3)
            .WithMessage("KYC level must be between 1 and 3");
    }
}

/// <summary>
/// Validator for SuspendCustomerCommand
/// Ensures customer suspension request is valid
/// </summary>
public class SuspendCustomerCommandValidator : AbstractValidator<SuspendCustomerCommand>
{
    public SuspendCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Customer ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for ActivateCustomerCommand
/// Ensures customer activation request is valid
/// </summary>
public class ActivateCustomerCommandValidator : AbstractValidator<ActivateCustomerCommand>
{
    public ActivateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("Customer ID must be a valid GUID");
    }
}
