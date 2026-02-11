using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Transactions.Api.DTOs;
using Xunit;

namespace Transactions.Api.IntegrationTests;

public class TransactionsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TransactionsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_Transactions_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateTransactionRequest(Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/transactions", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var location = response.Headers.Location;
        location.ShouldNotBeNull();
        location.ToString().ShouldContain("/api/transactions/");

        // Should be able to parse the transaction ID from the location
        var transactionIdString = location.ToString().Split('/').Last();
        Guid.TryParse(transactionIdString, out _).ShouldBeTrue();
    }

    [Fact]
    public async Task GET_Transactions_WithValidId_ShouldReturnTransaction()
    {
        // Arrange - Create a transaction first
        var customerId = Guid.NewGuid();
        var createRequest = new CreateTransactionRequest(customerId);

        var createResponse = await _client.PostAsJsonAsync("/api/transactions", createRequest);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!;
        var transactionId = Guid.Parse(location.ToString().Split('/').Last());

        // Act - Get the transaction
        var getResponse = await _client.GetAsync($"/api/transactions/{transactionId}");

        // Assert
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var transaction = await getResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        transaction.ShouldNotBeNull();
        transaction.Id.ShouldBe(transactionId);
        transaction.CustomerId.ShouldBe(customerId);
        transaction.Status.ShouldBe("Draft");
        transaction.TotalAmount.ShouldBe(0);
        transaction.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GET_Transactions_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Transactions_Items_ShouldAddItemToTransaction()
    {
        // Arrange - Create a transaction first
        var createRequest = new CreateTransactionRequest(Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/transactions", createRequest);
        var location = createResponse.Headers.Location!;
        var transactionId = Guid.Parse(location.ToString().Split('/').Last());

        var productId = Guid.NewGuid();
        var addItemRequest = new AddItemRequest(productId, "Test Product", 2, 15.99m);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/transactions/{transactionId}/items", addItemRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify item was added
        var getResponse = await _client.GetAsync($"/api/transactions/{transactionId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var transaction = await getResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        transaction.ShouldNotBeNull();
        transaction.Items.ShouldHaveSingleItem();
        transaction.TotalAmount.ShouldBe(31.98m); // 2 * 15.99

        var item = transaction.Items.Single();
        item.ProductId.ShouldBe(productId);
        item.ProductName.ShouldBe("Test Product");
        item.Quantity.ShouldBe(2);
        item.UnitPrice.ShouldBe(15.99m);
        item.TotalPrice.ShouldBe(31.98m);
    }

    [Fact]
    public async Task POST_Transactions_Submit_ShouldChangeStatus()
    {
        // Arrange - Create transaction and add item
        var createRequest = new CreateTransactionRequest(Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/transactions", createRequest);
        var location = createResponse.Headers.Location!;
        var transactionId = Guid.Parse(location.ToString().Split('/').Last());

        var addItemRequest = new AddItemRequest(Guid.NewGuid(), "Test Product", 1, 10.00m);

        await _client.PostAsJsonAsync($"/api/transactions/{transactionId}/items", addItemRequest);

        // Act - Submit transaction
        var submitResponse = await _client.PostAsync($"/api/transactions/{transactionId}/submit", null);

        // Assert
        submitResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify status changed
        var getResponse = await _client.GetAsync($"/api/transactions/{transactionId}");
        var transaction = await getResponse.Content.ReadFromJsonAsync<TransactionResponse>();
        transaction.Status.ShouldBe("Submitted");
        transaction.SubmittedAt.ShouldNotBeNull();
    }
}