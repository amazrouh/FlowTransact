using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Transactions.Application.Commands;
using Transactions.Application.Queries;
using Transactions.IntegrationTests.Fixtures;
using Xunit;

namespace Transactions.IntegrationTests.EndToEnd;

/// <summary>
/// E2E workflow tests using real PostgreSQL (Testcontainers).
/// Requires Docker to be available. Runs in CI when Docker is present.
/// </summary>
public class TransactionWorkflowE2ETests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;
    private readonly IMediator _mediator;

    public TransactionWorkflowE2ETests(PostgresFixture postgres)
    {
        _postgres = postgres;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));
        services.AddScoped<Transactions.Application.ITransactionRepository, Transactions.Infrastructure.Repositories.TransactionRepository>();
        services.AddDbContext<Transactions.Infrastructure.Persistence.TransactionsDbContext>(options =>
            options.UseNpgsql(_postgres.ConnectionString));

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task CompleteTransactionWorkflow_ShouldWorkEndToEnd()
    {

        var customerId = Guid.NewGuid();

        // Act 1: Create transaction
        var createCommand = new CreateTransactionCommand(customerId);
        var transactionId = await _mediator.Send(createCommand);

        transactionId.ShouldNotBe(Guid.Empty);

        // Act 2: Add items to transaction
        var addItemCommand1 = new AddTransactionItemCommand(
            transactionId,
            Guid.NewGuid(),
            "Product A",
            2,
            15.99m);

        var addItemCommand2 = new AddTransactionItemCommand(
            transactionId,
            Guid.NewGuid(),
            "Product B",
            1,
            29.99m);

        await _mediator.Send(addItemCommand1);
        await _mediator.Send(addItemCommand2);

        // Act 3: Submit transaction
        var submitCommand = new SubmitTransactionCommand(transactionId);
        await _mediator.Send(submitCommand);

        // Act 4: Query transaction
        var query = new GetTransactionQuery(transactionId);
        var retrievedTransaction = await _mediator.Send(query);

        retrievedTransaction.ShouldNotBeNull();
        retrievedTransaction!.Id.ShouldBe(transactionId);
        retrievedTransaction.Status.ShouldBe(Transactions.Domain.Enums.TransactionStatus.Submitted.ToString());
        retrievedTransaction.Items.Count.ShouldBe(2);
        retrievedTransaction.TotalAmount.ShouldBe(61.97m);
    }
}
