using MediatR;
using Microsoft.Extensions.Logging;
using Payments.Application.Exceptions;
using Payments.Domain.Aggregates;

namespace Payments.Application.Commands.Handlers;

public class StartPaymentCommandHandler : IRequestHandler<StartPaymentCommand, StartPaymentResult>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionApiClient _transactionApiClient;
    private readonly ILogger<StartPaymentCommandHandler> _logger;

    public StartPaymentCommandHandler(IPaymentRepository paymentRepository, ITransactionApiClient transactionApiClient, ILogger<StartPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _transactionApiClient = transactionApiClient;
        _logger = logger;
    }

    public async Task<StartPaymentResult> Handle(StartPaymentCommand request, CancellationToken cancellationToken)
    {
        decimal amount;
        if (request.Amount.HasValue)
        {
            // Event path: amount from trusted event, no fetch needed
            amount = request.Amount.Value;
        }
        else
        {
            // Manual API path: fetch transaction, validate customerId (fraud protection), get amount
            var transaction = await _transactionApiClient.GetTransactionAsync(request.TransactionId, cancellationToken);
            if (transaction is null)
                throw new KeyNotFoundException($"Transaction {request.TransactionId} not found.");

            if (transaction.CustomerId != request.CustomerId)
                throw new CustomerMismatchException("CustomerId does not match the transaction owner. Payment cannot be initiated.");

            if (transaction.Status != "Submitted")
                throw new InvalidOperationException($"Transaction must be in Submitted status to start payment. Current status: {transaction.Status}.");

            amount = transaction.TotalAmount;
        }

        // Idempotency: if payment already exists for this transaction, return existing
        var existing = await _paymentRepository.GetByTransactionIdAsync(request.TransactionId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation("Payment already exists for transaction: {TransactionId}, PaymentId: {PaymentId}", request.TransactionId, existing.Id);
            return new StartPaymentResult(existing.Id, true);
        }

        try
        {
            var payment = new Payment(request.TransactionId, request.CustomerId, amount);
            await _paymentRepository.AddAsync(payment, cancellationToken);
            _logger.LogInformation("Payment started: {PaymentId}, TransactionId: {TransactionId}, Amount: {Amount}", payment.Id, request.TransactionId, amount);
            return new StartPaymentResult(payment.Id, false);
        }
        catch (DuplicatePaymentException)
        {
            // Race: consumer created payment first. Return existing.
            existing = await _paymentRepository.GetByTransactionIdAsync(request.TransactionId, cancellationToken);
            if (existing is not null)
                return new StartPaymentResult(existing.Id, true);
            throw;
        }
    }
}
