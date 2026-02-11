using MediatR;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class AddTransactionItemCommandHandler : IRequestHandler<AddTransactionItemCommand>
{
    private readonly ITransactionRepository _transactionRepository;

    public AddTransactionItemCommandHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task Handle(AddTransactionItemCommand request, CancellationToken cancellationToken)
    {
        // Use the repository method that handles the entire operation atomically
        await _transactionRepository.AddItemToTransactionAsync(
            request.TransactionId,
            request.ProductId,
            request.ProductName,
            request.Quantity,
            request.UnitPrice,
            cancellationToken);
    }
}