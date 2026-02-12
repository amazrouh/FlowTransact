namespace Payments.Api.DTOs;

public record StartPaymentResponse(Guid PaymentId, bool AlreadyExisted, string? Message = null);
