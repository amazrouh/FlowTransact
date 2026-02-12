using MediatR;

namespace Payments.Application.Commands;

public record ConfirmPaymentCommand(Guid PaymentId) : IRequest;
