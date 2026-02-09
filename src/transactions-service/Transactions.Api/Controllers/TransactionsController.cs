using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.DTOs;
using Transactions.Application.Commands;
using Transactions.Application.Queries;
using Transactions.Domain.Aggregates;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;

namespace Transactions.Api.Controllers;

[ApiController]
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
        var query = new GetTransactionQuery(id);
        var transaction = await _mediator.Send(query);

        if (transaction is null)
        {
            return NotFound();
        }

        try
        {
            transaction.AddItem(request.ProductId, request.ProductName, request.Quantity, request.UnitPrice);
            // Note: In a real implementation, you'd have a command for adding items
            // For now, we'll assume the transaction is updated through the repository in the infrastructure
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitTransaction(Guid id)
    {
        try
        {
            var command = new SubmitTransactionCommand(id);
            await _mediator.Send(command);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var query = new GetTransactionQuery(id);
        var transaction = await _mediator.Send(query);

        if (transaction is null)
        {
            return NotFound();
        }

        var response = MapToResponse(transaction);
        return Ok(response);
    }

    private static TransactionResponse MapToResponse(Transaction transaction)
    {
        return new TransactionResponse(
            transaction.Id,
            transaction.CustomerId,
            transaction.Status.ToString(),
            transaction.TotalAmount,
            transaction.CreatedAt,
            transaction.SubmittedAt,
            transaction.CompletedAt,
            transaction.Items.Select(MapToItemDto).ToList());
    }

    private static TransactionItemDto MapToItemDto(TransactionItem item)
    {
        return new TransactionItemDto(
            item.Id,
            item.ProductId,
            item.ProductName,
            item.Quantity,
            item.UnitPrice,
            item.TotalPrice);
    }
}