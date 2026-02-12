using MassTransit;

namespace MoneyFellows.Contracts.Events;

[MessageUrn("MoneyFellows.Contracts.Events:PaymentConfirmed")]
public record PaymentConfirmed(
    Guid PaymentId,
    Guid TransactionId,
    decimal Amount,
    DateTime ConfirmedAt) : DomainEvent
{
    public override string EventType => "PaymentConfirmed";
}