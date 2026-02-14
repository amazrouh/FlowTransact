using MediatR;
using Payments.Application.Queries;
using Payments.Domain.Aggregates;

namespace Payments.Application.Queries.Handlers;

public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, PaymentDto?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        return payment is null ? null : MapToDto(payment);
    }

    private static PaymentDto MapToDto(Payment payment) =>
        new PaymentDto(
            payment.Id,
            payment.TransactionId,
            payment.CustomerId,
            payment.Amount,
            payment.Status.ToString(),
            payment.CreatedAt,
            payment.CompletedAt,
            payment.FailureReason);
}
