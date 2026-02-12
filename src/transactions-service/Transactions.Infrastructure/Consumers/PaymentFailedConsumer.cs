using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyFellows.Contracts.Events;
using Transactions.Application;

namespace Transactions.Infrastructure.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(ITransactionRepository transactionRepository, ILogger<PaymentFailedConsumer> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var message = context.Message;
        _logger.LogWarning(
            "Payment failed for Transaction {TransactionId}: {Reason}. PaymentId: {PaymentId}, Amount: {Amount}",
            message.TransactionId,
            message.Reason,
            message.PaymentId,
            message.Amount);

        var transaction = await _transactionRepository.GetByIdAsync(message.TransactionId, context.CancellationToken);
        if (transaction is null)
        {
            _logger.LogWarning("PaymentFailed received for unknown Transaction {TransactionId}", message.TransactionId);
            return;
        }

        if (transaction.Status == Transactions.Domain.Enums.TransactionStatus.Cancelled)
        {
            _logger.LogDebug("Transaction {TransactionId} already cancelled, skipping (idempotent)", message.TransactionId);
            return;
        }

        transaction.Cancel();
        await _transactionRepository.UpdateAsync(transaction, context.CancellationToken);
        _logger.LogInformation("Transaction {TransactionId} marked as cancelled after payment failure", message.TransactionId);
    }
}
