using MediatR;

namespace Transactions.Application.Commands;

public record SubmitTransactionCommand(
    Guid TransactionId) : IRequest;