using MediatR;

namespace Payments.Application.Commands.Handlers;

public class FailPaymentCommandHandler : IRequestHandler<FailPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentEventPublisher _eventPublisher;

    public FailPaymentCommandHandler(IPaymentRepository paymentRepository, IPaymentEventPublisher eventPublisher)
    {
        _paymentRepository = paymentRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
            throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found");

        payment.Fail(request.Reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _eventPublisher.PublishFailedAsync(payment.Id, payment.TransactionId, payment.Amount, request.Reason, cancellationToken);
    }
}
