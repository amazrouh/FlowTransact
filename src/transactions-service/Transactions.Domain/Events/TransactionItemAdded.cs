using MoneyFellows.Contracts.Events;

namespace Transactions.Domain.Events;

public record TransactionItemAdded(
    Guid TransactionId,
    Guid ItemId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice) : DomainEvent
{
    public override string EventType => "TransactionItemAdded";
}