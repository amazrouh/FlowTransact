using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Payments.Api.DTOs;
using Shouldly;
using Xunit;

namespace Payments.Api.IntegrationTests;

public class PaymentsApiTests : IClassFixture<PaymentsWebApplicationFactory>
{
    private readonly PaymentsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PaymentsApiTests(PaymentsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_Payments_Start_ShouldReturn201Created()
    {
        var transactionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _factory.SetupTransaction(transactionId, customerId, 99.99m);

        var request = new StartPaymentRequest(transactionId, customerId);

        var response = await _client.PostAsJsonAsync("/api/payments/start", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var location = response.Headers.Location;
        location.ShouldNotBeNull();
        location.ToString().ShouldContain("/api/payments/");

        var content = await response.Content.ReadFromJsonAsync<StartPaymentResponse>();
        content.ShouldNotBeNull();
        content.PaymentId.ShouldNotBe(Guid.Empty);
        content.AlreadyExisted.ShouldBeFalse();
    }

    [Fact]
    public async Task POST_Payments_Start_WhenAlreadyExisted_ShouldReturn200Ok()
    {
        var transactionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _factory.SetupTransaction(transactionId, customerId, 50.00m);

        var request = new StartPaymentRequest(transactionId, customerId);

        var response1 = await _client.PostAsJsonAsync("/api/payments/start", request);
        response1.StatusCode.ShouldBe(HttpStatusCode.Created);

        var response2 = await _client.PostAsJsonAsync("/api/payments/start", request);
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response2.Content.ReadFromJsonAsync<StartPaymentResponse>();
        content.ShouldNotBeNull();
        content.AlreadyExisted.ShouldBeTrue();
    }

    [Fact]
    public async Task GET_Payments_WithValidId_ShouldReturnPayment()
    {
        var transactionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _factory.SetupTransaction(transactionId, customerId, 25.50m);

        var startResponse = await _client.PostAsJsonAsync("/api/payments/start", new StartPaymentRequest(transactionId, customerId));
        var startContent = await startResponse.Content.ReadFromJsonAsync<StartPaymentResponse>();
        var paymentId = startContent!.PaymentId;

        var getResponse = await _client.GetAsync($"/api/payments/{paymentId}");

        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var payment = await getResponse.Content.ReadFromJsonAsync<PaymentResponse>();
        payment.ShouldNotBeNull();
        payment.Id.ShouldBe(paymentId);
        payment.TransactionId.ShouldBe(transactionId);
        payment.CustomerId.ShouldBe(customerId);
        payment.Amount.ShouldBe(25.50m);
        payment.Status.ShouldBe("Pending");
        payment.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public async Task GET_Payments_WithInvalidId_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/payments/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Payments_Confirm_ShouldChangeStatus()
    {
        var transactionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _factory.SetupTransaction(transactionId, customerId, 10.00m);

        var startResponse = await _client.PostAsJsonAsync("/api/payments/start", new StartPaymentRequest(transactionId, customerId));
        var startContent = await startResponse.Content.ReadFromJsonAsync<StartPaymentResponse>();
        var paymentId = startContent!.PaymentId;

        var confirmResponse = await _client.PostAsync($"/api/payments/{paymentId}/confirm", null);

        confirmResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/payments/{paymentId}");
        var payment = await getResponse.Content.ReadFromJsonAsync<PaymentResponse>();
        payment!.Status.ShouldBe("Completed");
        payment.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task POST_Payments_Fail_ShouldChangeStatus()
    {
        var transactionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _factory.SetupTransaction(transactionId, customerId, 75.00m);

        var startResponse = await _client.PostAsJsonAsync("/api/payments/start", new StartPaymentRequest(transactionId, customerId));
        var startContent = await startResponse.Content.ReadFromJsonAsync<StartPaymentResponse>();
        var paymentId = startContent!.PaymentId;

        var failResponse = await _client.PostAsJsonAsync($"/api/payments/{paymentId}/fail", new FailPaymentRequest("Card declined"));

        failResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/payments/{paymentId}");
        var payment = await getResponse.Content.ReadFromJsonAsync<PaymentResponse>();
        payment!.Status.ShouldBe("Failed");
        payment.FailureReason.ShouldBe("Card declined");
        payment.CompletedAt.ShouldNotBeNull();
    }
}
