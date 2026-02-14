using Microsoft.EntityFrameworkCore;
using Transactions.Application;
using Transactions.Infrastructure.Persistence;

namespace Transactions.Infrastructure.Diagnostics;

public class DiagnosticsService : IDiagnosticsService
{
    private readonly TransactionsDbContext _dbContext;

    public DiagnosticsService(TransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OutboxDiagnosticsResult> GetOutboxStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // MassTransit EF creates "OutboxMessage" table (PascalCase) - use quoted identifiers for PostgreSQL
            var totalCount = await _dbContext.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM \"OutboxMessage\"")
                .FirstOrDefaultAsync(cancellationToken);

            var recent = await _dbContext.Database
                .SqlQueryRaw<OutboxMessageRow>(
                    @"SELECT ""SequenceNumber"" AS ""SequenceNumber"", ""MessageId"" AS ""MessageId"", ""MessageType"" AS ""MessageType"", ""SentTime"" AS ""SentTime"", ""DestinationAddress"" AS ""DestinationAddress""
                      FROM ""OutboxMessage""
                      ORDER BY ""SequenceNumber"" DESC
                      LIMIT 10")
                .ToListAsync(cancellationToken);

            var hint = totalCount > 0
                ? "Messages stuck in outbox. Check MassTransit logs for delivery errors. Try Messaging:UseBusOutbox=false to bypass outbox."
                : "Outbox is empty. If TransactionSubmitted still not received, check RabbitMQ exchange bindings.";

            return new OutboxDiagnosticsResult(totalCount, recent, hint);
        }
        catch (Exception ex)
        {
            return new OutboxDiagnosticsResult(
                0,
                Array.Empty<OutboxMessageRow>(),
                $"Error: {ex.Message}. Ensure OutboxMessage table exists. Check connection string.");
        }
    }
}
