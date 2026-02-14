using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Transactions.Infrastructure.Persistence;

namespace Transactions.IntegrationTests.Fixtures;

/// <summary>
/// Fixture that starts a PostgreSQL container for integration tests.
/// Uses Testcontainers - requires Docker to be available (e.g. in CI).
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString => _container?.GetConnectionString() ?? throw new InvalidOperationException("Container not started");

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("FlowTransact_Test")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        await _container.StartAsync();

        using (var context = CreateContext())
        {
            context.Database.EnsureCreated();
        }
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
            await _container.DisposeAsync();
    }

    public TransactionsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TransactionsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var context = new TransactionsDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
