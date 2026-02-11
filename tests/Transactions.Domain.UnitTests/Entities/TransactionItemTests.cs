using Shouldly;
using Transactions.Domain.Entities;
using Xunit;

namespace Transactions.Domain.UnitTests.Entities;

public class TransactionItemTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateItem()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 3;
        var unitPrice = 25.99m;

        // Act
        var item = new TransactionItem(transactionId, productId, productName, quantity, unitPrice);

        // Assert
        item.Id.ShouldNotBe(Guid.Empty);
        item.ProductId.ShouldBe(productId);
        item.ProductName.ShouldBe(productName);
        item.Quantity.ShouldBe(quantity);
        item.UnitPrice.ShouldBe(unitPrice);
        item.TotalPrice.ShouldBe(77.97m); // 3 * 25.99
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(2, 15.50, 31.00)]
    [InlineData(5, 7.99, 39.95)]
    public void TotalPrice_ShouldCalculateCorrectly(int quantity, decimal unitPrice, decimal expectedTotal)
    {
        // Act
        var item = new TransactionItem(Guid.NewGuid(), Guid.NewGuid(), "Test Product", quantity, unitPrice);

        // Assert
        item.TotalPrice.ShouldBe(expectedTotal);
    }

    [Fact]
    public void Items_WithSameData_ShouldHaveUniqueIds()
    {
        // Act
        var transactionId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item1 = new TransactionItem(transactionId, productId, "Test Product", 1, 10.00m);
        var item2 = new TransactionItem(transactionId, productId, "Test Product", 1, 10.00m);

        // Assert
        item1.Id.ShouldNotBe(item2.Id);
        item1.Id.ShouldNotBe(Guid.Empty);
        item2.Id.ShouldNotBe(Guid.Empty);
    }
}