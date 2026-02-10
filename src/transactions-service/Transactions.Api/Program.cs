using Transactions.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Infrastructure layer (must be first as it sets up DbContext)
builder.Services.AddInfrastructure(builder.Configuration);

// Application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Transactions.Application.Commands.CreateTransactionCommand).Assembly));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
