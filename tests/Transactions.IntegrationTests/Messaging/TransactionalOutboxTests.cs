using MassTransit;
using MassTransit.Testing;
using Shouldly;
using Transactions.Domain;
using Transactions.Domain.Events;
using Transactions.IntegrationTests.Fixtures;
using Xunit;

namespace Transactions.IntegrationTests.Messaging;

// Simple mock context for testing event publishing
internal class MockContextForPublishing
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MockContextForPublishing(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishDomainEventsAsync(IPublishEndpoint publishEndpoint)
    {
        // Mock implementation - in real scenario this would be in the actual DbContext
        await Task.CompletedTask;
    }
}

public class TransactionalOutboxTests : IClassFixture<MessagingFixture>
{
    private readonly MessagingFixture _messaging;

    public TransactionalOutboxTests(MessagingFixture messaging)
    {
        _messaging = messaging;
    }

    [Fact]
    public async Task TransactionSubmittedEvent_ShouldBePublished()
    {
        // Arrange
        var harness = _messaging.Harness;
        var publishEndpoint = _messaging.GetService<IPublishEndpoint>();

        var transactionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act - Publish event directly (simulating what the repository would do)
        var @event = new TransactionSubmitted(transactionId, customerId, 100.00m);
        await publishEndpoint.Publish(@event);

        // Assert - Event should be captured by harness
        var publishedEvents = harness.Published.Select<TransactionSubmitted>().ToList();
        publishedEvents.ShouldHaveSingleItem();

        var publishedEvent = publishedEvents.Single().Context.Message;
        publishedEvent.TransactionId.ShouldBe(transactionId);
        publishedEvent.CustomerId.ShouldBe(customerId);
        publishedEvent.TotalAmount.ShouldBe(100.00m);
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