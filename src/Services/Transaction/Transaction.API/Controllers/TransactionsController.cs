using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Transaction.Application.Queries;

namespace Transaction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get transaction history for an account
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountTransactions(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAccountTransactionsQuery
        {
            AccountId = accountId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var query = new GetTransactionByIdQuery { TransactionId = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Transaction not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get transactions by date range
    /// </summary>
    [HttpGet("account/{accountId}/range")]
    [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionsByDateRange(
        Guid accountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var query = new GetTransactionsByDateRangeQuery
        {
            AccountId = accountId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
