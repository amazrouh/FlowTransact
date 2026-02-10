using Transactions.Domain.Aggregates;

using MediatR;

namespace Transactions.Application.Queries;

public record GetTransactionQuery(
    Guid TransactionId) : IRequest<Transaction?>;