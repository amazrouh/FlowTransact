using MediatR;
using Transactions.Application.Commands;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Commands.Handlers;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly IRepository<Transaction> _transactionRepository;

    public CreateTransactionCommandHandler(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Transaction(request.CustomerId);
        await _transactionRepository.AddAsync(transaction, cancellationToken);
        return transaction.Id;
    }
}