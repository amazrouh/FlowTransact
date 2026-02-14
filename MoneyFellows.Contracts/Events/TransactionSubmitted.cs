using MassTransit;

namespace MoneyFellows.Contracts.Events;

public static class TransactionSubmittedConstants
{
    public const string MessageUrn = "MoneyFellows.Contracts.Events:TransactionSubmitted";
}

[MessageUrn(TransactionSubmittedConstants.MessageUrn)]
public record TransactionSubmitted(
    Guid TransactionId,
    Guid CustomerId,
    decimal TotalAmount) : DomainEvent
{
    public override string EventType => "TransactionSubmitted";
}