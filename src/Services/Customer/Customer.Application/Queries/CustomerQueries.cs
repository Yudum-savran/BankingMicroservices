using MediatR;

namespace Customer.Application.Queries;

public class GetCustomerByIdQuery : IRequest<CustomerDto?>
{
    public Guid CustomerId { get; set; }
}

public class CustomerDto
{
    public Guid Id { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public string KYCLevel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
