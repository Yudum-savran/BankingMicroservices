using MediatR;
using Customer.Application.Queries;
using Customer.Domain.Repositories;

namespace Customer.Application.Queries.Handlers;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null) return null;

        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Street = customer.Address.Street,
            City = customer.Address.City,
            State = customer.Address.State,
            PostalCode = customer.Address.PostalCode,
            Country = customer.Address.Country,
            IdentityNumber = customer.IdentityNumber,
            DateOfBirth = customer.DateOfBirth,
            Status = customer.Status.ToString(),
            KYCLevel = customer.KYCLevel.ToString(),
            CreatedAt = customer.CreatedAt
        };
    }
}
