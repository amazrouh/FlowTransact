using MediatR;
using Transactions.Application.Queries;
using Transactions.Domain.Aggregates;

namespace Transactions.Application.Queries.Handlers;

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, TransactionDto?>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionDto?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
            return null;

        return new TransactionDto(
            transaction.Id,
            transaction.CustomerId,
            transaction.Status.ToString(),
            transaction.TotalAmount,
            transaction.CreatedAt,
            transaction.SubmittedAt,
            transaction.CompletedAt,
            transaction.Items.Select(i => new TransactionItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice)).ToList());
    }
}