namespace Payments.Api.DTOs;

public record PaymentResponse(
    Guid Id,
    Guid TransactionId,
    Guid CustomerId,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? FailureReason);
