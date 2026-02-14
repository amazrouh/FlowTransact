namespace Payments.Application.Queries;

public record PaymentDto(
    Guid Id,
    Guid TransactionId,
    Guid CustomerId,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? FailureReason);
