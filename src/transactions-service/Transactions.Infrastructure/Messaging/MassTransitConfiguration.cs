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
            // Add consumers here if needed

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ"));

                // Configure transactional outbox
                cfg.UsePublishFilter(typeof(OutboxPublishFilter<>), context);
            });

            // Configure the outbox
            x.AddEntityFrameworkOutbox<Transactions.Infrastructure.Persistence.TransactionsDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
        });

        return services;
    }
}