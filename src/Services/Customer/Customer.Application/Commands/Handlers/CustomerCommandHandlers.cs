using MediatR;
using Microsoft.Extensions.Logging;
using Customer.Application.Commands;
using Customer.Domain.Entities;
using Customer.Domain.Repositories;
using Customer.Domain.ValueObjects;

namespace Customer.Application.Commands.Handlers;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork, ILogger<CreateCustomerCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateCustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingCustomer = await _repository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingCustomer != null)
                return new CreateCustomerResponse { Success = false, Message = "Email already exists" };

            var address = new Address(request.Street, request.City, request.State, request.PostalCode, request.Country);
            
            var customer = BankCustomer.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                address,
                request.IdentityNumber,
                request.DateOfBirth
            );

            await _repository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer created: {CustomerId}, Email: {Email}", customer.Id, customer.Email);

            return new CreateCustomerResponse
            {
                CustomerId = customer.Id,
                Success = true,
                Message = "Customer created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return new CreateCustomerResponse { Success = false, Message = ex.Message };
        }
    }
}

public class UpdateCustomerContactCommandHandler : IRequestHandler<UpdateCustomerContactCommand, bool>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerContactCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateCustomerContactCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null) return false;

        customer.UpdateContactInfo(request.Email, request.PhoneNumber, request.Address!);
        await _repository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class UpgradeKYCLevelCommandHandler : IRequestHandler<UpgradeKYCLevelCommand, bool>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeKYCLevelCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpgradeKYCLevelCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null) return false;

        customer.UpgradeKYCLevel((KYCLevel)request.NewLevel);
        await _repository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class SuspendCustomerCommandHandler : IRequestHandler<SuspendCustomerCommand, bool>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SuspendCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null) return false;

        customer.Suspend();
        await _repository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand, bool>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null) return false;

        customer.Activate();
        await _repository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
