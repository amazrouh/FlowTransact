using MediatR;

namespace Payments.Application.Commands.Handlers;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentEventPublisher _eventPublisher;

    public ConfirmPaymentCommandHandler(IPaymentRepository paymentRepository, IPaymentEventPublisher eventPublisher)
    {
        _paymentRepository = paymentRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
            throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found");

        payment.Confirm();
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _eventPublisher.PublishConfirmedAsync(payment.Id, payment.TransactionId, payment.Amount, cancellationToken);
    }
}
