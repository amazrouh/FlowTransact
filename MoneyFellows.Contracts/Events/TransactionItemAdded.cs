using MassTransit;

namespace MoneyFellows.Contracts.Events;

[MessageUrn("MoneyFellows.Contracts.Events:TransactionItemAdded")]
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
