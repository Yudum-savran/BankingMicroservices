using Account.Application.Commands;
using Account.Application.Queries;
using Account.Domain.Exceptions;
using Account.API.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Account.API.Controllers;

/// <summary>
/// Account Controller - API Layer
/// Uses MediatR to send Commands and Queries (CQRS)
/// Validation is handled by FluentValidation + MediatR Pipeline
/// Error handling is centralized in GlobalExceptionHandlingMiddleware
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT Authorization
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
    /// <param name="command">Account creation details</param>
    /// <returns>Created account information</returns>
    /// <response code="201">Account created successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateAccountResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        _logger.LogInformation("Creating account for customer {CustomerId}", command.CustomerId);

        // Validation is automatically handled by MediatR pipeline behavior
        var result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetAccountById),
            new { id = result.AccountId },
            result);
    }

    /// <summary>
    /// Get account by ID - CQRS Query
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Account details</returns>
    /// <response code="200">Account found</response>
    /// <response code="404">Account not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            throw new NotFoundException("Account", id);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get accounts by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of accounts for the customer</returns>
    /// <response code="200">Accounts retrieved successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Unauthorized</response>
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
    /// <param name="accountNumber">Account number (e.g., TR26000010000000000001)</param>
    /// <returns>Account details</returns>
    /// <response code="200">Account found</response>
    /// <response code="404">Account not found</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("number/{accountNumber}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountByNumber(string accountNumber)
    {
        var query = new GetAccountByNumberQuery(accountNumber);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            throw new NotFoundException("Account", accountNumber);
        }

        return Ok(result);
    }

    /// <summary>
    /// Deposit money into account - CQRS Command
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="request">Deposit details</param>
    /// <returns>Deposit operation result</returns>
    /// <response code="200">Deposit successful</response>
    /// <response code="400">Validation failed or insufficient funds</response>
    /// <response code="404">Account not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{id}/deposit")]
    [ProducesResponseType(typeof(DepositMoneyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] DepositRequest request)
    {
        var command = new DepositMoneyCommand
        {
            AccountId = id,
            Amount = request.Amount,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Withdraw money from account - CQRS Command
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="request">Withdrawal details</param>
    /// <returns>Withdrawal operation result</returns>
    /// <response code="200">Withdrawal successful</response>
    /// <response code="400">Validation failed or insufficient funds</response>
    /// <response code="404">Account not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{id}/withdraw")]
    [ProducesResponseType(typeof(WithdrawMoneyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] WithdrawRequest request)
    {
        var command = new WithdrawMoneyCommand
        {
            AccountId = id,
            Amount = request.Amount,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Transfer money between accounts - CQRS Command
    /// </summary>
    /// <param name="id">Source account ID</param>
    /// <param name="request">Transfer details</param>
    /// <returns>Transfer operation result</returns>
    /// <response code="200">Transfer successful</response>
    /// <response code="400">Validation failed or insufficient funds</response>
    /// <response code="404">Source or target account not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{id}/transfer")]
    [ProducesResponseType(typeof(TransferMoneyResponse), StatusCodes.Status200OK)]
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

        return Ok(result);
    }
}

// Request DTOs
public record DepositRequest(decimal Amount, string Description);
public record WithdrawRequest(decimal Amount, string Description);
public record TransferRequest(Guid TargetAccountId, decimal Amount, string Description);
