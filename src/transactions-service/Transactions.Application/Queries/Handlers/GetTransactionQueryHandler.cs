using Transactions.Application.Queries;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Queries.Handlers;

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, Transaction?>
{
    private readonly IRepository<Transaction> _transactionRepository;

    public GetTransactionQueryHandler(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Transaction?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        return await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
    }
}