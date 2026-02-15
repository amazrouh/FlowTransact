using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using MoneyFellows.Contracts.Events;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class AddTransactionItemCommandHandler : IRequestHandler<AddTransactionItemCommand>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AddTransactionItemCommandHandler> _logger;

    public AddTransactionItemCommandHandler(ITransactionRepository transactionRepository, IPublishEndpoint publishEndpoint, ILogger<AddTransactionItemCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(AddTransactionItemCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
            throw new KeyNotFoundException($"Transaction with ID {request.TransactionId} not found");

        transaction.AddItem(
            request.ProductId,
            request.ProductName,
            request.Quantity,
            request.UnitPrice);

        var item = transaction.Items.Last();

        // Publish before SaveChanges so outbox messages are persisted in the same transaction
        await _publishEndpoint.Publish(new TransactionItemAdded(transaction.Id, item.Id, request.ProductId, request.ProductName, request.Quantity, request.UnitPrice), cancellationToken);
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        _logger.LogInformation("Item added to transaction: {TransactionId}, ItemId: {ItemId}, ProductName: {ProductName}, Quantity: {Quantity}",
            request.TransactionId, item.Id, request.ProductName, request.Quantity);
    }
}