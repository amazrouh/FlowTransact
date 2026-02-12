using MassTransit;
using MediatR;
using MoneyFellows.Contracts.Events;
using Payments.Application.Commands;

namespace Payments.Infrastructure.Consumers;

public class TransactionSubmittedConsumer : IConsumer<TransactionSubmitted>
{
    private readonly IMediator _mediator;

    public TransactionSubmittedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<TransactionSubmitted> context)
    {
        var message = context.Message;
        await _mediator.Send(new StartPaymentCommand(
            message.TransactionId,
            message.CustomerId,
            message.TotalAmount), context.CancellationToken);
    }
}
