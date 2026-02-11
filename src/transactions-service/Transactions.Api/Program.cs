using FluentValidation;
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

// Add health checks
builder.Services.AddHealthChecks();

// Register middleware
builder.Services.AddTransient<GlobalExceptionHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add middleware in correct order
app.UseMiddleware<CorrelationIdMiddleware>(); // Must be early for correlation ID
app.UseMiddleware<GlobalExceptionHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
    if (app.Environment.IsDevelopment())
    {
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
}

app.Run();

// Expose for WebApplicationFactory<Program> in integration tests
public partial class Program { }
