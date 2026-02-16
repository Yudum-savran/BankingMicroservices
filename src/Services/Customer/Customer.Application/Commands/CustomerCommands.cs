using MediatR;
using Customer.Domain.ValueObjects;

namespace Customer.Application.Commands;

public class CreateCustomerCommand : IRequest<CreateCustomerResponse>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class UpdateCustomerContactCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Address? Address { get; set; }
}

public class UpgradeKYCLevelCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }
    public int NewLevel { get; set; }
}

public class SuspendCustomerCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }
}

public class ActivateCustomerCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }
}

public class CreateCustomerResponse
{
    public Guid CustomerId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
