namespace Transactions.Application;

public interface IDiagnosticsService
{
    Task<OutboxDiagnosticsResult> GetOutboxStatsAsync(CancellationToken cancellationToken = default);
}

public record OutboxDiagnosticsResult(
    int TotalPending,
    IReadOnlyList<OutboxMessageRow> Recent,
    string Hint);

public record OutboxMessageRow(
    long SequenceNumber,
    Guid MessageId,
    string MessageType,
    DateTime SentTime,
    string? DestinationAddress);
