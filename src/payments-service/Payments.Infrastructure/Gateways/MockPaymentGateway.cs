namespace Payments.Infrastructure.Gateways;

public class MockPaymentGateway : IMockPaymentGateway
{
    public Task<MockPaymentIntentResult> CreateIntentAsync(Guid paymentId, Guid transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        // Mock always succeeds - returns a fake external ID
        return Task.FromResult(new MockPaymentIntentResult(Success: true, ExternalId: $"mock_{paymentId:N}"));
    }
}
