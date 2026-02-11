using MassTransit;
using Transactions.Application;
using Transactions.Domain.Aggregates;
using Transactions.Infrastructure.Persistence;

namespace Transactions.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionsDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public TransactionRepository(TransactionsDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events after saving
        await _context.PublishDomainEventsAsync(_publishEndpoint, cancellationToken);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events after saving
        await _context.PublishDomainEventsAsync(_publishEndpoint, cancellationToken);
    }
}