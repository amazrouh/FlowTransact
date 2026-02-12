namespace Payments.Infrastructure.Gateways;

public interface IMockPaymentGateway
{
    /// <summary>
    /// Simulates creating a payment intent. For mock, always returns success.
    /// </summary>
    Task<MockPaymentIntentResult> CreateIntentAsync(Guid paymentId, Guid transactionId, decimal amount, CancellationToken cancellationToken = default);
}

public record MockPaymentIntentResult(bool Success, string? ExternalId = null);
