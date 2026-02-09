using MassTransit;

namespace Transactions.Infrastructure.Messaging;

public class OutboxPublishFilter<T> :
    IFilter<PublishContext<T>>
    where T : class
{
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("outbox");
    }

    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        // The outbox will handle publishing, so we just continue
        await next.Send(context);
    }
}