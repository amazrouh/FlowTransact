using MassTransit;

namespace MoneyFellows.Contracts.Events;

[MessageUrn("MoneyFellows.Contracts.Events:PaymentFailed")]
public record PaymentFailed(
    Guid PaymentId,
    Guid TransactionId,
    decimal Amount,
    DateTime FailedAt,
    string Reason) : DomainEvent
{
    public override string EventType => "PaymentFailed";
}