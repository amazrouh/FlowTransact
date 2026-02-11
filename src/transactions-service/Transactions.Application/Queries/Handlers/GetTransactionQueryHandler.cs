using MediatR;
using Transactions.Application.Queries;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Queries.Handlers;

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, Transaction?>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Transaction?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        return await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
    }
}