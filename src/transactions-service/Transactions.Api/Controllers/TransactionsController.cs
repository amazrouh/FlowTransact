using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.DTOs;
using Transactions.Application.Commands;
using Transactions.Application.Queries;
using Transactions.Domain.Aggregates;
using Transactions.Domain.Entities;

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
        try
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
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (ArgumentException ex)
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