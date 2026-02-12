using Payments.Domain.Enums;

namespace Payments.Domain.Aggregates;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid TransactionId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment() { } // EF Core constructor

    public Payment(Guid transactionId, Guid customerId, decimal amount)
    {
        if (transactionId == Guid.Empty)
            throw new ArgumentException("TransactionId cannot be empty", nameof(transactionId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Id = Guid.NewGuid();
        TransactionId = transactionId;
        CustomerId = customerId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be confirmed");

        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be failed");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(reason));

        Status = PaymentStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        FailureReason = reason;
    }
}
