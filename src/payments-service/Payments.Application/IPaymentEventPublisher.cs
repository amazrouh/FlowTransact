namespace Payments.Application;

public interface IPaymentEventPublisher
{
    Task PublishConfirmedAsync(Guid paymentId, Guid transactionId, decimal amount, CancellationToken cancellationToken = default);
    Task PublishFailedAsync(Guid paymentId, Guid transactionId, decimal amount, string reason, CancellationToken cancellationToken = default);
}
