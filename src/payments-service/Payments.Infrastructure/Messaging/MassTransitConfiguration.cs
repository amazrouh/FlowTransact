using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyFellows.Contracts.Events;
using Payments.Infrastructure.Consumers;

namespace Payments.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TransactionSubmittedConsumer>();

            x.AddEntityFrameworkOutbox<Payments.Infrastructure.Persistence.PaymentsDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(1);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ"));

                cfg.ReceiveEndpoint("transaction-submitted", e =>
                {
                    e.UseEntityFrameworkOutbox<Payments.Infrastructure.Persistence.PaymentsDbContext>(context);
                    // Explicit bind: Publish sends to message-type exchange (single source of truth in contract).
                    e.Bind(TransactionSubmittedConstants.MessageUrn);
                    e.ConfigureConsumer<TransactionSubmittedConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);

                // Retry policy: 3 retries with incremental delay (1s, 6s, 11s). Skip retry for validation/business exceptions.
                cfg.UseMessageRetry(retry =>
                {
                    retry.Ignore<ArgumentException>();
                    retry.Ignore<InvalidOperationException>();
                    retry.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
                });
            });
        });

        return services;
    }
}
