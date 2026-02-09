namespace MoneyFellows.Contracts.Events;

public record TransactionSubmitted(
    Guid TransactionId,
    Guid CustomerId,
    decimal TotalAmount) : DomainEvent
{
    public override string EventType => "TransactionSubmitted";
}