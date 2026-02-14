using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Api.DTOs;
using Payments.Api.Middleware;
using Payments.Application.Commands;
using Payments.Application.Queries;

namespace Payments.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/payments")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Starts a payment for a submitted transaction. Returns 201 when created, 200 when payment already existed.</summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartPaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(StartPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartPayment([FromBody] StartPaymentRequest request)
    {
        var command = new StartPaymentCommand(request.TransactionId, request.CustomerId);
        var result = await _mediator.Send(command);

        var response = new StartPaymentResponse(
            PaymentId: result.PaymentId,
            AlreadyExisted: result.AlreadyExisted,
            Message: result.AlreadyExisted ? "Payment was already initiated for this transaction" : null);

        if (result.AlreadyExisted)
            return Ok(response);

        return CreatedAtAction(
            nameof(GetPayment),
            new { id = result.PaymentId },
            response);
    }

    /// <summary>Confirms a payment.</summary>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmPayment(Guid id)
    {
        var command = new ConfirmPaymentCommand(id);
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>Marks a payment as failed with a reason.</summary>
    [HttpPost("{id}/fail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FailPayment(Guid id, [FromBody] FailPaymentRequest request)
    {
        var command = new FailPaymentCommand(id, request.Reason);
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>Gets a payment by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var query = new GetPaymentQuery(id);
        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound();

        var response = new PaymentResponse(
            result.Id,
            result.TransactionId,
            result.CustomerId,
            result.Amount,
            result.Status,
            result.CreatedAt,
            result.CompletedAt,
            result.FailureReason);
        return Ok(response);
    }
}
