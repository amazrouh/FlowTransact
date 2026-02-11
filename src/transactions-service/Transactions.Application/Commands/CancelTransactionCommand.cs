using MediatR;

namespace Transactions.Application.Commands;

public record CancelTransactionCommand(
    Guid TransactionId) : IRequest;
