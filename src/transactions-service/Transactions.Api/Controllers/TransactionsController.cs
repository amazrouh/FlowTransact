using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.DTOs;
using Transactions.Application.Commands;
using Transactions.Application.Queries;

namespace Transactions.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var command = new CreateTransactionCommand(request.CustomerId);
        var transactionId = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetTransaction),
            new { id = transactionId },
            new { TransactionId = transactionId });
    }

    [HttpPost("{id}/items")]
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

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitTransaction(Guid id)
    {
        var command = new SubmitTransactionCommand(id);
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelTransaction(Guid id)
    {
        var command = new CancelTransactionCommand(id);
        await _mediator.Send(command);
        return Ok();
    }

    [HttpGet("{id}")]
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