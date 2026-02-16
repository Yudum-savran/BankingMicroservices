using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Customer.Application.Commands;
using Customer.Application.Queries;
using Customer.Domain.ValueObjects;

namespace Customer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateCustomerResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.CustomerId }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery { CustomerId = id });
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}/contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] UpdateContactRequest request)
    {
        var address = new Address(request.Street, request.City, request.State, request.PostalCode, request.Country);
        var command = new UpdateCustomerContactCommand
        {
            CustomerId = id,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = address
        };
        var result = await _mediator.Send(command);
        return result ? Ok() : NotFound();
    }

    [HttpPut("{id}/kyc-upgrade")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpgradeKYC(Guid id, [FromBody] UpgradeKYCRequest request)
    {
        var result = await _mediator.Send(new UpgradeKYCLevelCommand { CustomerId = id, NewLevel = request.NewLevel });
        return result ? Ok() : NotFound();
    }

    [HttpPost("{id}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var result = await _mediator.Send(new SuspendCustomerCommand { CustomerId = id });
        return result ? Ok() : NotFound();
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _mediator.Send(new ActivateCustomerCommand { CustomerId = id });
        return result ? Ok() : NotFound();
    }
}

public record UpdateContactRequest(string Email, string PhoneNumber, string Street, string City, string State, string PostalCode, string Country);
public record UpgradeKYCRequest(int NewLevel);
