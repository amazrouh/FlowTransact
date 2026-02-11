using DotNet.Testcontainers.Containers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Transactions.Application;
using Transactions.Domain.Aggregates;
using Transactions.Infrastructure.Messaging;
using Transactions.Infrastructure.Repositories;

namespace Transactions.IntegrationTests.Fixtures;

public class MessagingFixture : IAsyncLifetime
{
    private readonly RabbitMqTestcontainer _container;
    private IServiceProvider _serviceProvider;
    private ITestHarness _harness;

    public string ConnectionString => _container.ConnectionString;
    public ITestHarness Harness => _harness;

    public MessagingFixture()
    {
        _container = new RabbitMqTestcontainer()
            .WithUsername("guest")
            .WithPassword("guest")
            .WithCleanUp(true);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var services = new ServiceCollection();

        // Configure MassTransit for testing
        services.AddMassTransitTestHarness(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(_container.ConnectionString);

                // Configure transactional outbox
                cfg.UsePublishFilter(typeof(OutboxPublishFilter<>), context);
            });

            // Configure the outbox
            x.AddEntityFrameworkOutbox<TransactionsDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
        });

        // Add test services
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        _serviceProvider = services.BuildServiceProvider();

        _harness = _serviceProvider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _container.DisposeAsync();
    }

    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}