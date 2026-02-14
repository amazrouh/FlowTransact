using MassTransit;
using Microsoft.EntityFrameworkCore;
using Transactions.Application;
using Transactions.Domain.Aggregates;
using Transactions.Infrastructure.Persistence;

namespace Transactions.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionsDbContext _context;
    private readonly IPublishEndpoint? _publishEndpoint;

    public TransactionRepository(TransactionsDbContext context, IPublishEndpoint? publishEndpoint = null)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        // Publish domain events BEFORE SaveChanges - required for MassTransit UseBusOutbox.
        // Publishing during SaveChanges interceptor can deadlock with outbox infrastructure.
        await _context.PublishDomainEventsAsync(_publishEndpoint, null, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        // Explicitly add any new items (not yet in DB) - avoids EF generating UPDATE instead of INSERT
        foreach (var item in transaction.Items)
        {
            var itemEntry = _context.Entry(item);
            if (itemEntry.State == EntityState.Detached)
            {
                _context.TransactionItems.Add(item);
            }
        }

        // Publish domain events BEFORE SaveChanges - required for MassTransit UseBusOutbox.
        await _context.PublishDomainEventsAsync(_publishEndpoint, null, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}