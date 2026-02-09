namespace Transactions.Application.Commands;

public record CreateTransactionCommand(
    Guid CustomerId) : IRequest<Guid>;