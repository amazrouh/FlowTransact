using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transactions.Application;
using Transactions.Domain.Aggregates;
using Transactions.Infrastructure.Diagnostics;
using Transactions.Infrastructure.Messaging;
using Transactions.Infrastructure.Persistence;
using Transactions.Infrastructure.Repositories;

namespace Transactions.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext options and register context with factory so IPublishEndpoint is injected.
        // Domain events are published from SaveChangesAsync override BEFORE base.SaveChanges (required for UseBusOutbox).
        services.AddDbContext<TransactionsDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<TransactionsDbContext>(sp =>
        {
            var options = sp.GetRequiredService<DbContextOptions<TransactionsDbContext>>();
            var publishEndpoint = sp.GetService<IPublishEndpoint>();
            return new TransactionsDbContext(options, publishEndpoint);
        });

        // Add repositories
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Add diagnostics (debug endpoints)
        services.AddScoped<IDiagnosticsService, DiagnosticsService>();

        // Add MassTransit
        services.AddMassTransitConfiguration(configuration);

        return services;
    }
}