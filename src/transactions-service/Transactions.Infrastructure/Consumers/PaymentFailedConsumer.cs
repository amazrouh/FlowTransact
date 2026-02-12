using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyFellows.Contracts.Events;

namespace Transactions.Infrastructure.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(ILogger<PaymentFailedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var message = context.Message;
        _logger.LogWarning(
            "Payment failed for Transaction {TransactionId}: {Reason}. PaymentId: {PaymentId}, Amount: {Amount}",
            message.TransactionId,
            message.Reason,
            message.PaymentId,
            message.Amount);

        // Per DOMAIN.md: Transaction stays Submitted until Completed or Cancelled.
        // No state change for PaymentFailed - just log for observability.
        return Task.CompletedTask;
    }
}
