using MassTransit.Testing;
using Shouldly;
using Transactions.Domain.Events;
using Transactions.IntegrationTests.Fixtures;
using Xunit;

namespace Transactions.IntegrationTests.Messaging;

public class TransactionalOutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _database;

    public TransactionalOutboxTests(DatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task DomainEvents_ShouldBeRaisedOnTransactionOperations()
    {
        // Arrange
        var harness = new InMemoryTestHarness();
        await harness.Start();

        var context = _database.CreateContext();
        var transaction = new Transactions.Domain.Aggregates.Transaction(Guid.NewGuid());

        // Act - Add item (should raise event)
        transaction.AddItem(Guid.NewGuid(), "Test Product", 1, 10.00m);
        transaction.Submit();

        // Publish events
        await context.PublishDomainEventsAsync(harness.Bus);

        // Assert - Events should be published
        var submittedEvents = harness.Published.Select<TransactionSubmitted>().ToList();
        var itemAddedEvents = harness.Published.Select<TransactionItemAdded>().ToList();

        submittedEvents.ShouldHaveSingleItem();
        itemAddedEvents.ShouldHaveSingleItem();

        var submittedEvent = submittedEvents.Single().Context.Message;
        submittedEvent.TransactionId.ShouldBe(transaction.Id);
        submittedEvent.TotalAmount.ShouldBe(10.00m);

        var itemAddedEvent = itemAddedEvents.Single().Context.Message;
        itemAddedEvent.TransactionId.ShouldBe(transaction.Id);
        itemAddedEvent.ProductName.ShouldBe("Test Product");
    }

    [Fact]
    public async Task FailedTransaction_ShouldNotPublishEvents()
    {
        // Arrange
        var harness = new InMemoryTestHarness();
        await harness.Start();

        var transaction = new Transactions.Domain.Aggregates.Transaction(Guid.NewGuid());

        // Act & Assert - Submit without items should fail
        Should.Throw<InvalidOperationException>(() => transaction.Submit());

        // No events should be published
        transaction.DomainEvents.ShouldBeEmpty();
    }
}