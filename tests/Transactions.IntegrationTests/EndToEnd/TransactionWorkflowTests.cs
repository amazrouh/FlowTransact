using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Transactions.Application.Commands;
using Transactions.Application.Queries;
using Transactions.IntegrationTests.Fixtures;
using Xunit;

namespace Transactions.IntegrationTests.EndToEnd;

public class TransactionWorkflowTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _database;

    public TransactionWorkflowTests(DatabaseFixture database)
    {
        _database = database;
    }

    [Fact(Skip = "EF InMemory DbUpdateConcurrencyException when AddItem runs in new scope - manual/Docker flow works")]
    public async Task CompleteTransactionWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange - shared database name ensures all scopes see same InMemory data
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));
        services.AddScoped<Transactions.Application.ITransactionRepository, Transactions.Infrastructure.Repositories.TransactionRepository>();
        services.AddScoped(_ => _database.CreateContext());
        services.AddMassTransitTestHarness();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var customerId = Guid.NewGuid();

        // Act 1: Create transaction
        var createCommand = new CreateTransactionCommand(customerId);
        var transactionId = await mediator.Send(createCommand);

        // Assert 1: Transaction created
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

        await mediator.Send(addItemCommand1);
        await mediator.Send(addItemCommand2);

        // Act 3: Submit transaction
        var submitCommand = new SubmitTransactionCommand(transactionId);
        await mediator.Send(submitCommand);

        // Assert 3 & Act 4: Query transaction
        var query = new GetTransactionQuery(transactionId);
        var retrievedTransaction = await mediator.Send(query);

        // Assert 4: Transaction query works
        retrievedTransaction.ShouldNotBeNull();
        retrievedTransaction.Id.ShouldBe(transactionId);
        retrievedTransaction.Status.ShouldBe(Transactions.Domain.Enums.TransactionStatus.Submitted.ToString());
        retrievedTransaction!.Items.Count.ShouldBe(2);
        retrievedTransaction.TotalAmount.ShouldBe(61.97m);
    }

    [Fact]
    public async Task FailedOperation_ShouldNotLeaveSystemInInconsistentState()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));
        services.AddScoped<Transactions.Application.ITransactionRepository, Transactions.Infrastructure.Repositories.TransactionRepository>();
        services.AddScoped(_ => _database.CreateContext());
        services.AddMassTransitTestHarness();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var customerId = Guid.NewGuid();

        // Create a valid transaction
        var createCommand = new CreateTransactionCommand(customerId);
        var transactionId = await mediator.Send(createCommand);

        var context = serviceProvider.GetRequiredService<Transactions.Infrastructure.Persistence.TransactionsDbContext>();
        var transaction = await context.Transactions.FindAsync(transactionId);
        transaction.ShouldNotBeNull();
        transaction.Status.ShouldBe(Transactions.Domain.Enums.TransactionStatus.Draft);

        // Act - Try to submit transaction with no items (should fail)
        var invalidSubmitCommand = new SubmitTransactionCommand(transactionId);

        // Assert - Command should fail
        await Should.ThrowAsync<InvalidOperationException>(
            () => mediator.Send(invalidSubmitCommand));

        // Verify transaction state was not changed (rollback behavior)
        await context.Entry(transaction).ReloadAsync();
        transaction.Status.ShouldBe(Transactions.Domain.Enums.TransactionStatus.Draft);
        transaction.SubmittedAt.ShouldBeNull();

        // Verify we can still use the transaction
        var addItemCommand = new AddTransactionItemCommand(
            transactionId,
            Guid.NewGuid(),
            "Recovery Product",
            1,
            25.00m);

        await mediator.Send(addItemCommand);

        // Verify transaction is still usable after failed operation
        var query = new GetTransactionQuery(transactionId);
        var retrieved = await mediator.Send(query);
        retrieved!.Status.ShouldBe(Transactions.Domain.Enums.TransactionStatus.Draft.ToString());
        retrieved.Items.Count.ShouldBe(1);
        retrieved.TotalAmount.ShouldBe(25.00m);
    }
}