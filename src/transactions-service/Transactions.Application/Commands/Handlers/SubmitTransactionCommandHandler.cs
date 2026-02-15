using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using MoneyFellows.Contracts.Events;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class SubmitTransactionCommandHandler : IRequestHandler<SubmitTransactionCommand>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SubmitTransactionCommandHandler> _logger;

    public SubmitTransactionCommandHandler(ITransactionRepository transactionRepository, IPublishEndpoint publishEndpoint, ILogger<SubmitTransactionCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(SubmitTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Transaction with ID {request.TransactionId} not found");
        }

        transaction.Submit();

        // Publish before SaveChanges so outbox messages are persisted in the same transaction
        await _publishEndpoint.Publish(new TransactionSubmitted(transaction.Id, transaction.CustomerId, transaction.TotalAmount), cancellationToken);
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        _logger.LogInformation("Transaction submitted: {TransactionId}, TotalAmount: {TotalAmount}", request.TransactionId, transaction.TotalAmount);
    }
}