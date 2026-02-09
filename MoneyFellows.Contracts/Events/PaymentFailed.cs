namespace MoneyFellows.Contracts.Events;

public record PaymentFailed(
    Guid PaymentId,
    Guid TransactionId,
    decimal Amount,
    DateTime FailedAt,
    string Reason) : DomainEvent
{
    public override string EventType => "PaymentFailed";
}