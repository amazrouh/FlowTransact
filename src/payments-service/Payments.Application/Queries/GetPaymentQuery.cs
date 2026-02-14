using MediatR;

namespace Payments.Application.Queries;

public record GetPaymentQuery(Guid PaymentId) : IRequest<PaymentDto?>;
