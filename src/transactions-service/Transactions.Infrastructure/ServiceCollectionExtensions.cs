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
        // Add DbContext - domain events are published from repository BEFORE SaveChanges
        // (interceptor removed to avoid UseBusOutbox deadlock when publishing during SaveChanges)
        services.AddDbContext<TransactionsDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
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