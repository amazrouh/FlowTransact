namespace Transactions.Domain.Events;

public record TransactionSubmittedDomainEvent(
    Guid TransactionId,
    Guid CustomerId,
    decimal TotalAmount) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType => "TransactionSubmitted";
}
