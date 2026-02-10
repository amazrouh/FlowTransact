using MediatR;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class AddTransactionItemCommandHandler : IRequestHandler<AddTransactionItemCommand>
{
    private readonly IRepository<Transaction> _transactionRepository;

    public AddTransactionItemCommandHandler(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task Handle(AddTransactionItemCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Transaction with ID {request.TransactionId} not found");
        }

        transaction.AddItem(request.ProductId, request.ProductName, request.Quantity, request.UnitPrice);
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
    }
}