using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions.Infrastructure.Persistence;

namespace Transactions.Api.Controllers;

/// <summary>
/// Debug endpoints for TransactionSubmitted and outbox diagnostics.
/// Use when Publish fails to reach Payments consumer.
/// </summary>
[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly TransactionsDbContext _dbContext;

    public DiagnosticsController(TransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns outbox message stats (pending, recent) for debugging.
    /// If messages are stuck in outbox_message, they are not being delivered to RabbitMQ.
    /// </summary>
    [HttpGet("outbox")]
    public async Task<IActionResult> GetOutboxStats(CancellationToken cancellationToken)
    {
        try
        {
            var totalCount = await _dbContext.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) FROM outbox_message")
                .FirstOrDefaultAsync(cancellationToken);

            var recent = await _dbContext.Database
                .SqlQueryRaw<OutboxMessageRow>(
                    @"SELECT sequence_number AS ""SequenceNumber"", message_id AS ""MessageId"", message_type AS ""MessageType"", sent_time AS ""SentTime"", destination_address AS ""DestinationAddress""
                      FROM outbox_message
                      ORDER BY sequence_number DESC
                      LIMIT 10")
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                TotalPending = totalCount,
                Recent = recent,
                Hint = totalCount > 0
                    ? "Messages stuck in outbox. Check MassTransit logs for delivery errors. Try Messaging:UseBusOutbox=false to bypass outbox."
                    : "Outbox is empty. If TransactionSubmitted still not received, check RabbitMQ exchange bindings."
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                Error = ex.Message,
                Hint = "Ensure outbox_message table exists. Check connection string."
            });
        }
    }

    private record OutboxMessageRow(long SequenceNumber, Guid MessageId, string MessageType, DateTime SentTime, string? DestinationAddress);
}
