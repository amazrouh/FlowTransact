using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.RabbitMq;
using Transactions.Application;
using Transactions.Infrastructure.Messaging;
using Transactions.Infrastructure.Persistence;
using Transactions.Infrastructure.Repositories;

namespace Transactions.IntegrationTests.Fixtures;

public class MessagingFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container;
    private IServiceProvider? _serviceProvider;
    private ITestHarness? _harness;

    public string ConnectionString => _container.GetConnectionString();
    public ITestHarness Harness => _harness!;

    public MessagingFixture()
    {
        _container = new RabbitMqBuilder("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
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
                cfg.Host(_container.GetConnectionString());

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

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public T GetService<T>() where T : notnull
    {
        return _serviceProvider!.GetRequiredService<T>();
    }
}