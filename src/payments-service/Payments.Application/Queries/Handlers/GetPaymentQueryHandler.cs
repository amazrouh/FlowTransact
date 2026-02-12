using MediatR;
using Payments.Domain.Aggregates;

namespace Payments.Application.Queries.Handlers;

public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Payment?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Payment?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
    }
}
