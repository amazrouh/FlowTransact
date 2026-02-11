using Transactions.Domain.Aggregates;

namespace Transactions.Application;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task AddItemToTransactionAsync(Guid transactionId, Guid productId, string productName, int quantity, decimal unitPrice, CancellationToken cancellationToken = default);
}