using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transactions.Domain.Events;

namespace Transactions.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Configure message retry and error handling
            x.AddDelayedMessageScheduler();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ"));

                // Configure transactional outbox
                cfg.UsePublishFilter(typeof(OutboxPublishFilter<>), context);

                // Configure retry policies
                cfg.UseMessageRetry(retry => retry
                    .Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));

                // Configure dead letter queues
                cfg.UseDelayedMessageScheduler();
            });

            // Configure the outbox with PostgreSQL
            x.AddEntityFrameworkOutbox<Transactions.Infrastructure.Persistence.TransactionsDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(5); // Check for new messages every 5 seconds
            });

            // Add delayed message scheduler
            x.AddDelayedMessageScheduler();
        });

        return services;
    }
}