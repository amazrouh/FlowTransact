using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Api.DTOs;
using Payments.Application.Commands;
using Payments.Application.Exceptions;
using Payments.Application.Queries;
using Payments.Domain.Aggregates;

namespace Payments.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartPayment([FromBody] StartPaymentRequest request)
    {
        try
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
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (CustomerMismatchException ex)
        {
            return StatusCode(403, new { Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmPayment(Guid id)
    {
        try
        {
            var command = new ConfirmPaymentCommand(id);
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

    [HttpPost("{id}/fail")]
    public async Task<IActionResult> FailPayment(Guid id, [FromBody] FailPaymentRequest request)
    {
        try
        {
            var command = new FailPaymentCommand(id, request.Reason);
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var query = new GetPaymentQuery(id);
        var payment = await _mediator.Send(query);

        if (payment is null)
            return NotFound();

        return Ok(MapToResponse(payment));
    }

    private static PaymentResponse MapToResponse(Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.TransactionId,
            payment.CustomerId,
            payment.Amount,
            payment.Status.ToString(),
            payment.CreatedAt,
            payment.CompletedAt,
            payment.FailureReason);
    }
}
