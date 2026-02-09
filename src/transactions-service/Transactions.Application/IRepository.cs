using Transactions.Domain;

namespace Transactions.Application;

public interface IRepository<T> where T : class, IHasDomainEvents
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
}