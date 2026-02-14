using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Payments.Application;

namespace Payments.Api.IntegrationTests;

public class PaymentsWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MockTransactionApiClient _mockTransactionApi = new();

    /// <summary>
    /// Configure a transaction to be returned when StartPayment fetches it.
    /// Call this before tests that need StartPayment to succeed.
    /// </summary>
    public void SetupTransaction(Guid transactionId, Guid customerId, decimal totalAmount, string status = "Submitted")
    {
        _mockTransactionApi.RegisterTransaction(transactionId, customerId, totalAmount, status);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ITransactionApiClient>();
            services.AddSingleton<ITransactionApiClient>(_mockTransactionApi);
        });
    }
}
