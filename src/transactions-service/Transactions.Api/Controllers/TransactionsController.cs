using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.DTOs;
using Transactions.Api.Middleware;
using Transactions.Application.Commands;
using Transactions.Application.Queries;

namespace Transactions.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new transaction for the given customer.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var command = new CreateTransactionCommand(request.CustomerId);
        var transactionId = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetTransaction),
            new { id = transactionId },
            new { TransactionId = transactionId });
    }

    /// <summary>Adds an item to an existing transaction.</summary>
    [HttpPost("{id}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddItemRequest request)
    {
        var command = new AddTransactionItemCommand(
            TransactionId: id,
            ProductId: request.ProductId,
            ProductName: request.ProductName,
            Quantity: request.Quantity,
            UnitPrice: request.UnitPrice);

        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>Submits a transaction for processing.</summary>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitTransaction(Guid id)
    {
        var command = new SubmitTransactionCommand(id);
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>Cancels a transaction.</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelTransaction(Guid id)
    {
        var command = new CancelTransactionCommand(id);
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>Gets a transaction by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var query = new GetTransactionQuery(id);
        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound();

        // Map Application DTO to API response
        var response = new TransactionResponse(
            result.Id,
            result.CustomerId,
            result.Status,
            result.TotalAmount,
            result.CreatedAt,
            result.SubmittedAt,
            result.CompletedAt,
            result.Items.Select(i => new DTOs.TransactionItemDto(
                i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());

        return Ok(response);
    }
}