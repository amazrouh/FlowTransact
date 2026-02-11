using Shouldly;
using Transactions.Domain.Events;
using Xunit;

namespace Transactions.Domain.UnitTests.Events;

public class DomainEventTests
{
    [Fact]
    public void TransactionItemAdded_ShouldContainRequiredData()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 2;
        var unitPrice = 15.99m;

        // Act
        var @event = new TransactionItemAdded(
            transactionId,
            itemId,
            productId,
            productName,
            quantity,
            unitPrice);

        // Assert
        @event.TransactionId.ShouldBe(transactionId);
        @event.ItemId.ShouldBe(itemId);
        @event.ProductId.ShouldBe(productId);
        @event.ProductName.ShouldBe(productName);
        @event.Quantity.ShouldBe(quantity);
        @event.UnitPrice.ShouldBe(unitPrice);
        @event.EventId.ShouldNotBe(Guid.Empty);
        @event.OccurredOn.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        @event.EventType.ShouldBe("TransactionItemAdded");
    }

    [Fact]
    public void TransactionSubmitted_ShouldContainRequiredData()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var totalAmount = 99.99m;

        // Act
        var @event = new TransactionSubmitted(transactionId, customerId, totalAmount);

        // Assert
        @event.TransactionId.ShouldBe(transactionId);
        @event.CustomerId.ShouldBe(customerId);
        @event.TotalAmount.ShouldBe(totalAmount);
        @event.EventId.ShouldNotBe(Guid.Empty);
        @event.OccurredOn.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        @event.EventType.ShouldBe("TransactionSubmitted");
    }

    [Fact]
    public void TransactionItemAdded_ShouldHaveUniqueEventIds()
    {
        // Arrange & Act
        var event1 = new TransactionItemAdded(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Product A", 1, 10.00m);
        var event2 = new TransactionItemAdded(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Product B", 1, 15.00m);

        // Assert
        event1.EventId.ShouldNotBe(event2.EventId);
        event1.EventId.ShouldNotBe(Guid.Empty);
        event2.EventId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void DomainEvents_ShouldHaveReasonableTimestamps()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var @event = new TransactionSubmitted(Guid.NewGuid(), Guid.NewGuid(), 100.00m);
        var afterCreation = DateTime.UtcNow;

        // Assert
        @event.OccurredOn.ShouldBeGreaterThanOrEqualTo(beforeCreation);
        @event.OccurredOn.ShouldBeLessThanOrEqualTo(afterCreation);
    }
}