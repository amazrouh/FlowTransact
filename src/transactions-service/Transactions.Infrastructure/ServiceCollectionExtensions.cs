using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transactions.Application;
using Transactions.Domain.Aggregates;
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
        // Add DbContext
        services.AddDbContext<TransactionsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add repositories
        services.AddScoped<IRepository<Transaction>, Repository<Transaction>>();

        // Add MassTransit
        services.AddMassTransitConfiguration(configuration);

        return services;
    }
}