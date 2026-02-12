using MediatR;
using Payments.Domain.Aggregates;

namespace Payments.Application.Queries;

public record GetPaymentQuery(Guid PaymentId) : IRequest<Payment?>;
