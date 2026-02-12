namespace Payments.Api.DTOs;

public record StartPaymentRequest(Guid TransactionId, Guid CustomerId);
