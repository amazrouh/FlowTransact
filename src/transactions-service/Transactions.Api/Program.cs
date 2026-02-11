using FluentValidation;
using Serilog;
using Transactions.Api.Middleware;
using Transactions.Api.Validators;
using Transactions.Infrastructure;

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

app.Run();
