using MediatR;

namespace Transactions.Application.Commands;

public record AddTransactionItemCommand(
    Guid TransactionId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice) : IRequest;