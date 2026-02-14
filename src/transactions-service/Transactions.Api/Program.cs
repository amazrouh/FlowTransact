using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Transactions.Api.Middleware;
using Transactions.Api.Validators;
using Transactions.Infrastructure;
using Transactions.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console());

// Add services to the container.

// Infrastructure layer (must be first as it sets up DbContext)
builder.Services.AddInfrastructure(builder.Configuration);

// Application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Transactions.Application.Commands.CreateTransactionCommand).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<AddTransactionItemCommandValidator>();

// Add validation pipeline behavior - runs validators before handlers
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Transactions.Api.Behaviors.ValidationBehavior<,>));

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgres",
        tags: new[] { "db", "ready" })
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration.GetConnectionString("RabbitMQ")!,
        name: "rabbitmq",
        tags: new[] { "messaging", "ready" });

// Register middleware
builder.Services.AddTransient<GlobalExceptionHandler>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV");

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FlowTransact Transactions API",
        Version = "v1",
        Description = "API for managing financial transactions in the MoneyFellows platform."
    });
});

var app = builder.Build();

// Add middleware in correct order
app.UseMiddleware<CorrelationIdMiddleware>(); // Must be early for correlation ID
app.UseMiddleware<GlobalExceptionHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Transactions API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var initContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
    try
    {
        if (app.Environment.IsDevelopment())
        {
            await initContext.Database.EnsureDeletedAsync();
            await initContext.Database.EnsureCreatedAsync();
        }
        else
        {
            await initContext.Database.EnsureCreatedAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}

app.Run();

// Expose for WebApplicationFactory<Program> in integration tests
public partial class Program { }
