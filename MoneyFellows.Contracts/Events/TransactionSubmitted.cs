using MassTransit;

namespace MoneyFellows.Contracts.Events;

[MessageUrn("MoneyFellows.Contracts.Events:TransactionSubmitted")]
[EntityName("transaction-submitted")]
public record TransactionSubmitted(
    Guid TransactionId,
    Guid CustomerId,
    decimal TotalAmount) : DomainEvent
{
    public override string EventType => "TransactionSubmitted";
}