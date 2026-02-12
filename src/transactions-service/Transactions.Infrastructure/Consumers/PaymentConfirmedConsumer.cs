using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyFellows.Contracts.Events;
using Transactions.Application;
using Transactions.Domain.Enums;

namespace Transactions.Infrastructure.Consumers;

public class PaymentConfirmedConsumer : IConsumer<PaymentConfirmed>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<PaymentConfirmedConsumer> _logger;

    public PaymentConfirmedConsumer(ITransactionRepository transactionRepository, ILogger<PaymentConfirmedConsumer> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentConfirmed> context)
    {
        var message = context.Message;
        var transaction = await _transactionRepository.GetByIdAsync(message.TransactionId, context.CancellationToken);

        if (transaction is null)
        {
            _logger.LogWarning("PaymentConfirmed received for unknown Transaction {TransactionId}", message.TransactionId);
            return;
        }

        if (transaction.Status == TransactionStatus.Completed)
        {
            _logger.LogDebug("Transaction {TransactionId} already completed, skipping (idempotent)", message.TransactionId);
            return;
        }

        transaction.MarkAsCompleted();
        await _transactionRepository.UpdateAsync(transaction, context.CancellationToken);
        _logger.LogInformation("Transaction {TransactionId} marked as completed", message.TransactionId);
    }
}
