using MediatR;

namespace Payments.Application.Commands;

public record FailPaymentCommand(Guid PaymentId, string Reason) : IRequest;
