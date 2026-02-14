using AutoFixture;
using Shouldly;
using Transactions.Domain.Aggregates;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;
using Transactions.Domain.Events;
using Xunit;

namespace Transactions.Domain.UnitTests.Aggregates;

public class TransactionTests
{
    private readonly Fixture _fixture;

    public TransactionTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Constructor_WithValidCustomerId_ShouldCreateDraftTransaction()
    {
        // Arrange
        var customerId = _fixture.Create<Guid>();

        // Act
        var transaction = new Transaction(customerId);

        // Assert
        transaction.Id.ShouldNotBe(Guid.Empty);
        transaction.CustomerId.ShouldBe(customerId);
        transaction.Status.ShouldBe(TransactionStatus.Draft);
        transaction.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        transaction.TotalAmount.ShouldBe(0);
        transaction.Items.ShouldBeEmpty();
        transaction.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void AddItem_WithValidData_ShouldAddItemAndRaiseEvent()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        var productId = _fixture.Create<Guid>();
        var productName = "Test Product";
        var quantity = 2;
        var unitPrice = 15.99m;

        // Act
        transaction.AddItem(productId, productName, quantity, unitPrice);

        // Assert
        transaction.Items.ShouldHaveSingleItem();
        var item = transaction.Items.Single();
        item.ProductId.ShouldBe(productId);
        item.ProductName.ShouldBe(productName);
        item.Quantity.ShouldBe(quantity);
        item.UnitPrice.ShouldBe(unitPrice);
        item.TotalPrice.ShouldBe(31.98m); // 2 * 15.99

        transaction.TotalAmount.ShouldBe(31.98m);

        transaction.DomainEvents.ShouldHaveSingleItem();
        var domainEvent = transaction.DomainEvents.Single();
        domainEvent.ShouldBeOfType<TransactionItemAddedDomainEvent>();
        var itemAddedEvent = (TransactionItemAddedDomainEvent)domainEvent;
        itemAddedEvent.TransactionId.ShouldBe(transaction.Id);
        itemAddedEvent.ItemId.ShouldBe(item.Id);
        itemAddedEvent.ProductId.ShouldBe(productId);
        itemAddedEvent.Quantity.ShouldBe(quantity);
        itemAddedEvent.UnitPrice.ShouldBe(unitPrice);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void AddItem_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            transaction.AddItem(_fixture.Create<Guid>(), "Test Product", invalidQuantity, 10.00m))
            .ParamName.ShouldBe("quantity");
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(0.0)]
    public void AddItem_WithInvalidUnitPrice_ShouldThrowArgumentException(decimal invalidUnitPrice)
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, invalidUnitPrice))
            .ParamName.ShouldBe("unitPrice");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AddItem_WithInvalidProductName_ShouldThrowArgumentException(string invalidProductName)
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            transaction.AddItem(_fixture.Create<Guid>(), invalidProductName, 1, 10.00m))
            .ParamName.ShouldBe("productName");
    }

    [Fact]
    public void AddItem_AfterSubmission_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, 10.00m);
        transaction.Submit(); // Change status to Submitted

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            transaction.AddItem(_fixture.Create<Guid>(), "Another Product", 1, 5.00m))
            .Message.ShouldBe("Items can only be added to draft transactions");
    }

    [Fact]
    public void Submit_WithValidTransaction_ShouldChangeStatusAndRaiseEvent()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 2, 15.00m);

        // Act
        transaction.Submit();

        // Assert
        transaction.Status.ShouldBe(TransactionStatus.Submitted);
        transaction.SubmittedAt.ShouldNotBeNull();
        transaction.SubmittedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        transaction.TotalAmount.ShouldBe(30.00m); // 2 * 15.00

        transaction.DomainEvents.Count.ShouldBe(2); // TransactionItemAdded + TransactionSubmitted
        var submittedEvent = transaction.DomainEvents
            .OfType<TransactionSubmittedDomainEvent>()
            .Single();
        submittedEvent.TransactionId.ShouldBe(transaction.Id);
        submittedEvent.CustomerId.ShouldBe(transaction.CustomerId);
        submittedEvent.TotalAmount.ShouldBe(30.00m);
    }

    [Fact]
    public void Submit_WithoutItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => transaction.Submit())
            .Message.ShouldBe("Cannot submit transaction without items");
    }

    [Theory]
    [InlineData(TransactionStatus.Submitted)]
    [InlineData(TransactionStatus.Completed)]
    public void Submit_AfterAlreadySubmitted_ShouldThrowInvalidOperationException(TransactionStatus invalidStatus)
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, 10.00m);

        // Set invalid status (simulate already submitted)
        typeof(Transaction).GetProperty("Status")!.SetValue(transaction, invalidStatus);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => transaction.Submit())
            .Message.ShouldBe("Only draft transactions can be submitted");
    }

    // Removed: Submit_WithZeroTotalAmount test - not possible due to AddItem validation

    [Theory]
    [InlineData(TransactionStatus.Draft)]
    [InlineData(TransactionStatus.Submitted)]
    public void Cancel_WithValidStatus_ShouldChangeStatus(TransactionStatus initialStatus)
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, 10.00m);

        // Set initial status
        typeof(Transaction).GetProperty("Status")!.SetValue(transaction, initialStatus);

        // Act
        transaction.Cancel();

        // Assert
        transaction.Status.ShouldBe(TransactionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, 10.00m);
        transaction.Submit();

        // Set to completed status
        typeof(Transaction).GetProperty("Status")!.SetValue(transaction, TransactionStatus.Completed);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => transaction.Cancel())
            .Message.ShouldBe("Only draft or submitted transactions can be cancelled");
    }

    [Fact]
    public void MarkAsCompleted_WithSubmittedTransaction_ShouldChangeStatus()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, 10.00m);
        transaction.Submit();

        // Act
        transaction.MarkAsCompleted();

        // Assert
        transaction.Status.ShouldBe(TransactionStatus.Completed);
        transaction.CompletedAt.ShouldNotBeNull();
        transaction.CompletedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Theory]
    [InlineData(TransactionStatus.Draft)]
    [InlineData(TransactionStatus.Cancelled)]
    public void MarkAsCompleted_WithInvalidStatus_ShouldThrowInvalidOperationException(TransactionStatus invalidStatus)
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, 10.00m);

        // Set invalid status
        typeof(Transaction).GetProperty("Status")!.SetValue(transaction, invalidStatus);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => transaction.MarkAsCompleted())
            .Message.ShouldBe("Only submitted transactions can be completed");
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());
        transaction.AddItem(_fixture.Create<Guid>(), "Test Product", 1, 10.00m);
        transaction.Submit();

        // Act
        transaction.ClearDomainEvents();

        // Assert
        transaction.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void TotalAmount_WithMultipleItems_ShouldCalculateCorrectly()
    {
        // Arrange
        var transaction = new Transaction(_fixture.Create<Guid>());

        // Act
        transaction.AddItem(_fixture.Create<Guid>(), "Product A", 2, 10.00m); // 20.00
        transaction.AddItem(_fixture.Create<Guid>(), "Product B", 1, 15.50m); // 15.50
        transaction.AddItem(_fixture.Create<Guid>(), "Product C", 3, 5.00m);  // 15.00
                                                                                 // Total: 50.50

        // Assert
        transaction.TotalAmount.ShouldBe(50.50m);
        transaction.Items.Count.ShouldBe(3);
    }
}