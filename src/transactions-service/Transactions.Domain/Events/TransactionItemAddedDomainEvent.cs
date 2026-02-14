namespace Transactions.Domain.Events;

public record TransactionItemAddedDomainEvent(
    Guid TransactionId,
    Guid ItemId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType => "TransactionItemAdded";
}
