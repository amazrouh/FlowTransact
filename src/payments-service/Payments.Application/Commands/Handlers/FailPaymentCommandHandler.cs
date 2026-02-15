using MediatR;
using Microsoft.Extensions.Logging;

namespace Payments.Application.Commands.Handlers;

public class FailPaymentCommandHandler : IRequestHandler<FailPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentEventPublisher _eventPublisher;
    private readonly ILogger<FailPaymentCommandHandler> _logger;

    public FailPaymentCommandHandler(IPaymentRepository paymentRepository, IPaymentEventPublisher eventPublisher, ILogger<FailPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
            throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found");

        payment.Fail(request.Reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _eventPublisher.PublishFailedAsync(payment.Id, payment.TransactionId, payment.Amount, request.Reason, cancellationToken);
        _logger.LogInformation("Payment failed: {PaymentId}, TransactionId: {TransactionId}, Reason: {Reason}", payment.Id, payment.TransactionId, request.Reason);
    }
}
