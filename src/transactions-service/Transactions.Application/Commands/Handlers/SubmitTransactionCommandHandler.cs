using MediatR;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class SubmitTransactionCommandHandler : IRequestHandler<SubmitTransactionCommand>
{
    private readonly IRepository<Transaction> _transactionRepository;

    public SubmitTransactionCommandHandler(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task Handle(SubmitTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Transaction with ID {request.TransactionId} not found");
        }

        transaction.Submit();
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);

        // Note: Domain events will be published by the infrastructure layer
        // through the outbox pattern when the transaction is saved
    }
}