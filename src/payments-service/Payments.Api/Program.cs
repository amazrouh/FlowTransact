using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Payments.Api.Middleware;
using Payments.Application.Behaviors;
using Payments.Application.Validators;
using Payments.Infrastructure;
using Payments.Infrastructure.Persistence;
using Serilog;
using Serilog.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Payments.Application.Commands.StartPaymentCommand).Assembly));

builder.Services.AddValidatorsFromAssemblyContaining<StartPaymentCommandValidator>();
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV");

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddTransient<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FlowTransact Payments API",
        Version = "v1",
        Description = "API for managing payments in the MoneyFellows platform."
    });
    options.OperationFilter<Payments.Api.Swagger.ErrorResponsesOperationFilter>();
    options.SchemaFilter<Payments.Api.Swagger.RequestExamplesSchemaFilter>();
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(); // Logs HTTP requests (method, path, status, duration) to Seq
app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    try
    {
        if (app.Environment.IsDevelopment())
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
        else
        {
            await dbContext.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}

app.Run();

public partial class Program { }
