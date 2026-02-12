namespace Payments.Application;

public record TransactionInfo(Guid CustomerId, decimal TotalAmount, string Status);

public interface ITransactionApiClient
{
    Task<TransactionInfo?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
