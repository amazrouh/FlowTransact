using MassTransit;
using Microsoft.EntityFrameworkCore;
using Transactions.Application;
using Transactions.Domain.Aggregates;
using Transactions.Domain.Entities;
using Transactions.Infrastructure.Persistence;

namespace Transactions.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionsDbContext _context;
    private readonly IPublishEndpoint? _publishEndpoint;
    private readonly ISendEndpointProvider? _sendEndpointProvider;

    public TransactionRepository(TransactionsDbContext context, IPublishEndpoint? publishEndpoint, ISendEndpointProvider? sendEndpointProvider)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _sendEndpointProvider = sendEndpointProvider;
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
        // Publish BEFORE SaveChanges so domain + outbox are committed in one transaction (Bus Outbox)
        await _context.PublishDomainEventsAsync(_publishEndpoint, _sendEndpointProvider, cancellationToken);
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

        // Publish BEFORE SaveChanges so domain + outbox are committed in one transaction (Bus Outbox)
        await _context.PublishDomainEventsAsync(_publishEndpoint, _sendEndpointProvider, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddItemToTransactionAsync(Guid transactionId, Guid productId, string productName, int quantity, decimal unitPrice, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions.FindAsync(new object[] { transactionId }, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Transaction with ID {transactionId} not found");
        }

        // Domain logic (validation, domain events)
        transaction.AddItem(productId, productName, quantity, unitPrice);

        // Explicitly add the new item - the last one added by AddItem
        var newItem = transaction.Items.Last();
        _context.TransactionItems.Add(newItem);

        // Publish BEFORE SaveChanges so domain + outbox are committed in one transaction (Bus Outbox)
        await _context.PublishDomainEventsAsync(_publishEndpoint, _sendEndpointProvider, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}