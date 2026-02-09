namespace MoneyFellows.Contracts.Events;

public record PaymentConfirmed(
    Guid PaymentId,
    Guid TransactionId,
    decimal Amount,
    DateTime ConfirmedAt) : DomainEvent
{
    public override string EventType => "PaymentConfirmed";
}