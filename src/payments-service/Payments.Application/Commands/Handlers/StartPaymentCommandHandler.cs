using MediatR;
using Payments.Application.Exceptions;
using Payments.Domain.Aggregates;

namespace Payments.Application.Commands.Handlers;

public class StartPaymentCommandHandler : IRequestHandler<StartPaymentCommand, StartPaymentResult>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionApiClient _transactionApiClient;

    public StartPaymentCommandHandler(IPaymentRepository paymentRepository, ITransactionApiClient transactionApiClient)
    {
        _paymentRepository = paymentRepository;
        _transactionApiClient = transactionApiClient;
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
            return new StartPaymentResult(existing.Id, true);

        var payment = new Payment(request.TransactionId, request.CustomerId, amount);
        await _paymentRepository.AddAsync(payment, cancellationToken);
        return new StartPaymentResult(payment.Id, false);
    }
}
