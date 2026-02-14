using Payments.Application;

namespace Payments.Api.IntegrationTests;

/// <summary>
/// Mock ITransactionApiClient for API integration tests. Returns valid transaction info
/// for any request so StartPayment can succeed without a real Transactions API.
/// </summary>
public class MockTransactionApiClient : ITransactionApiClient
{
    private readonly Dictionary<Guid, TransactionInfo> _transactions = new();

    /// <summary>
    /// Register a transaction that will be returned for the given transactionId.
    /// </summary>
    public void RegisterTransaction(Guid transactionId, Guid customerId, decimal totalAmount, string status = "Submitted")
    {
        _transactions[transactionId] = new TransactionInfo(customerId, totalAmount, status);
    }

    public Task<TransactionInfo?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        _transactions.TryGetValue(transactionId, out var info);
        return Task.FromResult<TransactionInfo?>(info);
    }
}
