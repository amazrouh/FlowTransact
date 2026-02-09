using Transactions.Domain.Aggregates;

namespace Transactions.Application.Queries;

public record GetTransactionQuery(
    Guid TransactionId) : IRequest<Transaction?>;