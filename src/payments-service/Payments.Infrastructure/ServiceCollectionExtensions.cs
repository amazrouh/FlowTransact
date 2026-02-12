using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application;
using Payments.Domain.Aggregates;
using Payments.Infrastructure.Gateways;
using Payments.Infrastructure.Messaging;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Repositories;

namespace Payments.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentEventPublisher, PaymentEventPublisher>();
        services.AddSingleton<IMockPaymentGateway, MockPaymentGateway>();

        services.Configure<TransactionApiOptions>(configuration.GetSection(TransactionApiOptions.SectionName));
        services.AddHttpClient<ITransactionApiClient, TransactionApiClient>();

        services.AddMassTransitConfiguration(configuration);

        return services;
    }
}
