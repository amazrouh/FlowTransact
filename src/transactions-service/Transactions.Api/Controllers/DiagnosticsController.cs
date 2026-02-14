using Microsoft.AspNetCore.Mvc;
using Transactions.Application;

namespace Transactions.Api.Controllers;

/// <summary>
/// Debug endpoints for TransactionSubmitted and outbox diagnostics.
/// Use when Publish fails to reach Payments consumer.
/// </summary>
[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticsService _diagnosticsService;

    public DiagnosticsController(IDiagnosticsService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService;
    }

    /// <summary>
    /// Returns outbox message stats (pending, recent) for debugging.
    /// If messages are stuck in outbox, they are not being delivered to RabbitMQ.
    /// </summary>
    [HttpGet("outbox")]
    public async Task<IActionResult> GetOutboxStats(CancellationToken cancellationToken)
    {
        var result = await _diagnosticsService.GetOutboxStatsAsync(cancellationToken);
        return Ok(new
        {
            TotalPending = result.TotalPending,
            Recent = result.Recent,
            Hint = result.Hint
        });
    }
}
