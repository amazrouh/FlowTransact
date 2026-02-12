using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transactions.Infrastructure.Consumers;

namespace Transactions.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentConfirmedConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();

            // Configure message retry and error handling
            x.AddDelayedMessageScheduler();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ"));

                // Explicitly configure payment event consumers to ensure correct queue binding
                cfg.ReceiveEndpoint("payment-confirmed", e =>
                {
                    e.ConfigureConsumer<PaymentConfirmedConsumer>(context);
                });
                cfg.ReceiveEndpoint("payment-failed", e =>
                {
                    e.ConfigureConsumer<PaymentFailedConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);

                // Configure retry policies
                cfg.UseMessageRetry(retry => retry
                    .Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));

                // Configure dead letter queues
                cfg.UseDelayedMessageScheduler();
            });

            // Configure the outbox with PostgreSQL (for PaymentConfirmed/Failed consumers).
            // Temporarily removed AddEntityFrameworkOutbox - the delivery service was faulting and may
            // have been interfering with direct publish. Transactions will publish directly to RabbitMQ.
            // x.AddEntityFrameworkOutbox<Transactions.Infrastructure.Persistence.TransactionsDbContext>(o =>
            // {
            //     o.UsePostgres();
            //     o.QueryDelay = TimeSpan.FromSeconds(1);
            // });

            // Add delayed message scheduler
            x.AddDelayedMessageScheduler();
        });

        return services;
    }
}