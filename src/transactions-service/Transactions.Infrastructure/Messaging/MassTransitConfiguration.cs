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

                // Explicitly configure payment event consumers with EF inbox for idempotency
                cfg.ReceiveEndpoint("payment-confirmed", e =>
                {
                    e.UseEntityFrameworkOutbox<Transactions.Infrastructure.Persistence.TransactionsDbContext>(context);
                    e.ConfigureConsumer<PaymentConfirmedConsumer>(context);
                });
                cfg.ReceiveEndpoint("payment-failed", e =>
                {
                    e.UseEntityFrameworkOutbox<Transactions.Infrastructure.Persistence.TransactionsDbContext>(context);
                    e.ConfigureConsumer<PaymentFailedConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);

                // Retry policy: 3 retries with incremental delay (1s, 6s, 11s). Skip retry for validation/business exceptions.
                cfg.UseMessageRetry(retry =>
                {
                    retry.Ignore<ArgumentException>();
                    retry.Ignore<InvalidOperationException>();
                    retry.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
                });

                // After retries exhausted, failed messages go to _error queue (MassTransit default)
                cfg.UseDelayedMessageScheduler();
            });

            // Entity Framework outbox for idempotent consumers (Inbox) and Bus Outbox.
            // UseBusOutbox: Send/ Publish go to outbox table; delivery service delivers to RabbitMQ.
            x.AddEntityFrameworkOutbox<Transactions.Infrastructure.Persistence.TransactionsDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(1);
            });

            // Add delayed message scheduler
            x.AddDelayedMessageScheduler();
        });

        return services;
    }
}