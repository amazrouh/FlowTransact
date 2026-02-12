using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyFellows.Contracts.Events;
using Payments.Application;

namespace Payments.Infrastructure.Messaging;

public class PaymentEventPublisher : IPaymentEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentEventPublisher> _logger;

    public PaymentEventPublisher(IPublishEndpoint publishEndpoint, ILogger<PaymentEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishConfirmedAsync(Guid paymentId, Guid transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        var evt = new PaymentConfirmed(paymentId, transactionId, amount, DateTime.UtcNow);
        await _publishEndpoint.Publish(evt, cancellationToken);
        _logger.LogInformation("Published PaymentConfirmed for Payment {PaymentId}, Transaction {TransactionId}", paymentId, transactionId);
    }

    public async Task PublishFailedAsync(Guid paymentId, Guid transactionId, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        var evt = new PaymentFailed(paymentId, transactionId, amount, DateTime.UtcNow, reason);
        await _publishEndpoint.Publish(evt, cancellationToken);
        _logger.LogInformation("Published PaymentFailed for Payment {PaymentId}, Transaction {TransactionId}: {Reason}", paymentId, transactionId, reason);
    }
}
