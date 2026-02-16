using Account.Application.Commands;
using Account.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Account.API.Controllers;

/// <summary>
/// Account Controller - API Layer
/// Uses MediatR to send Commands and Queries (CQRS)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT Authorization
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new bank account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        _logger.LogInformation("Creating account for customer {CustomerId}", command.CustomerId);

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetAccountById),
            new { id = result.AccountId },
            result);
    }

    /// <summary>
    /// Get account by ID - CQRS Query
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get accounts by customer ID
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(List<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsByCustomerId(Guid customerId)
    {
        var query = new GetAccountsByCustomerIdQuery(customerId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get account by account number
    /// </summary>
    [HttpGet("number/{accountNumber}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountByNumber(string accountNumber)
    {
        var query = new GetAccountByNumberQuery(accountNumber);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Deposit money - CQRS Command
    /// </summary>
    [HttpPost("{id}/deposit")]
    [ProducesResponseType(typeof(DepositMoneyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] DepositRequest request)
    {
        var command = new DepositMoneyCommand
        {
            AccountId = id,
            Amount = request.Amount,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Withdraw money - CQRS Command
    /// </summary>
    [HttpPost("{id}/withdraw")]
    [ProducesResponseType(typeof(WithdrawMoneyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] WithdrawRequest request)
    {
        var command = new WithdrawMoneyCommand
        {
            AccountId = id,
            Amount = request.Amount,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Transfer money between accounts - CQRS Command
    /// </summary>
    [HttpPost("{id}/transfer")]
    [ProducesResponseType(typeof(TransferMoneyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer(Guid id, [FromBody] TransferRequest request)
    {
        var command = new TransferMoneyCommand
        {
            SourceAccountId = id,
            TargetAccountId = request.TargetAccountId,
            Amount = request.Amount,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

// Request DTOs
public record DepositRequest(decimal Amount, string Description);
public record WithdrawRequest(decimal Amount, string Description);
public record TransferRequest(Guid TargetAccountId, decimal Amount, string Description);
