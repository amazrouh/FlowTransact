using Shouldly;
using Transactions.Application;
using Transactions.Domain.Aggregates;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;
using Transactions.IntegrationTests.Fixtures;
using Xunit;

namespace Transactions.IntegrationTests.Repositories;

public class TransactionRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _database;

    public TransactionRepositoryTests(DatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task CreateTransaction_ShouldPersistToDatabase()
    {
        // Arrange
        var context = _database.CreateContext();
        var repository = new Transactions.Infrastructure.Repositories.TransactionRepository(context, null!);
        var customerId = Guid.NewGuid();
        var transaction = new Transaction(customerId);

        // Act
        await repository.AddAsync(transaction);

        // Assert
        var savedTransaction = await context.Transactions.FindAsync(transaction.Id);

        savedTransaction.ShouldNotBeNull();
        savedTransaction.Id.ShouldBe(transaction.Id);
        savedTransaction.CustomerId.ShouldBe(customerId);
        savedTransaction.Status.ShouldBe(TransactionStatus.Draft);
        savedTransaction.CreatedAt.ShouldBe(transaction.CreatedAt);
        savedTransaction.TotalAmount.ShouldBe(0);
    }

    [Fact]
    public async Task UpdateTransaction_ShouldPersistChanges()
    {
        // Arrange
        var context = _database.CreateContext();
        var repository = new Transactions.Infrastructure.Repositories.TransactionRepository(context, null!);
        var customerId = Guid.NewGuid();
        var transaction = new Transaction(customerId);
        await repository.AddAsync(transaction);

        // Modify the transaction
        transaction.AddItem(Guid.NewGuid(), "Test Product", 2, 15.99m);
        transaction.Submit();

        // Act
        await repository.UpdateAsync(transaction);

        // Assert
        var updatedTransaction = await context.Transactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        updatedTransaction.ShouldNotBeNull();
        updatedTransaction.Status.ShouldBe(TransactionStatus.Submitted);
        updatedTransaction.SubmittedAt.ShouldNotBeNull();
        updatedTransaction.TotalAmount.ShouldBe(31.98m);
        updatedTransaction.Items.ShouldHaveSingleItem();

        var item = updatedTransaction.Items.Single();
        item.ProductName.ShouldBe("Test Product");
        item.Quantity.ShouldBe(2);
        item.UnitPrice.ShouldBe(15.99m);
    }

    [Fact]
    public async Task GetById_WithExistingTransaction_ShouldReturnTransaction()
    {
        // Arrange
        var context = _database.CreateContext();
        var repository = new Transactions.Infrastructure.Repositories.TransactionRepository(context, null!);
        var customerId = Guid.NewGuid();
        var transaction = new Transaction(customerId);
        await repository.AddAsync(transaction);

        // Act
        var retrievedTransaction = await repository.GetByIdAsync(transaction.Id);

        // Assert
        retrievedTransaction.ShouldNotBeNull();
        retrievedTransaction.Id.ShouldBe(transaction.Id);
        retrievedTransaction.CustomerId.ShouldBe(customerId);
        retrievedTransaction.Status.ShouldBe(TransactionStatus.Draft);
    }

    [Fact]
    public async Task GetById_WithNonExistingTransaction_ShouldReturnNull()
    {
        // Arrange
        var context = _database.CreateContext();
        var repository = new Transactions.Infrastructure.Repositories.TransactionRepository(context, null!);

        // Act
        var transaction = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        transaction.ShouldBeNull();
    }

    [Fact]
    public async Task TransactionWithItems_ShouldPersistItemsCorrectly()
    {
        // Arrange
        var context = _database.CreateContext();
        var repository = new Transactions.Infrastructure.Repositories.TransactionRepository(context, null!);
        var customerId = Guid.NewGuid();
        var transaction = new Transaction(customerId);

        transaction.AddItem(Guid.NewGuid(), "Product A", 1, 10.00m);
        transaction.AddItem(Guid.NewGuid(), "Product B", 2, 15.50m);
        transaction.AddItem(Guid.NewGuid(), "Product C", 3, 7.99m);

        // Act
        await repository.AddAsync(transaction);

        // Assert
        var savedTransaction = await context.Transactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        savedTransaction.ShouldNotBeNull();
        savedTransaction.Items.ShouldHaveCount(3);
        savedTransaction.TotalAmount.ShouldBe(10.00m + 31.00m + 23.97m); // 65.97

        var productA = savedTransaction.Items.First(i => i.ProductName == "Product A");
        productA.Quantity.ShouldBe(1);
        productA.UnitPrice.ShouldBe(10.00m);
        productA.TotalPrice.ShouldBe(10.00m);

        var productB = savedTransaction.Items.First(i => i.ProductName == "Product B");
        productB.Quantity.ShouldBe(2);
        productB.UnitPrice.ShouldBe(15.50m);
        productB.TotalPrice.ShouldBe(31.00m);
    }
}