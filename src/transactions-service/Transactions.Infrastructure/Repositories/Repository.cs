using MassTransit;
using Transactions.Application;
using Transactions.Domain;
using Transactions.Infrastructure.Persistence;

namespace Transactions.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class, IHasDomainEvents
{
    private readonly TransactionsDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public Repository(TransactionsDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events after saving
        await _context.PublishDomainEventsAsync(_publishEndpoint, cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events after saving
        await _context.PublishDomainEventsAsync(_publishEndpoint, cancellationToken);
    }
}