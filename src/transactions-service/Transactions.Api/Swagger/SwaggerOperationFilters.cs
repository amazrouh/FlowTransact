using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Transactions.Api.Swagger;

/// <summary>
/// Adds error response examples (400, 404, 409, 500) to all operations.
/// </summary>
public class ErrorResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses.TryAdd("400", new OpenApiResponse
        {
            Description = "Bad Request – validation failed or invalid operation",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Example = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["statusCode"] = new Microsoft.OpenApi.Any.OpenApiInteger(400),
                        ["message"] = new Microsoft.OpenApi.Any.OpenApiString("One or more validation errors occurred."),
                        ["path"] = new Microsoft.OpenApi.Any.OpenApiString("/api/transactions"),
                        ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O")),
                        ["errors"] = new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["CustomerId"] = new Microsoft.OpenApi.Any.OpenApiArray
                            {
                                new Microsoft.OpenApi.Any.OpenApiString("CustomerId is required")
                            }
                        }
                    }
                }
            }
        });

        operation.Responses.TryAdd("404", new OpenApiResponse
        {
            Description = "Not Found – resource does not exist",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Example = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["statusCode"] = new Microsoft.OpenApi.Any.OpenApiInteger(404),
                        ["message"] = new Microsoft.OpenApi.Any.OpenApiString("Transaction not found."),
                        ["path"] = new Microsoft.OpenApi.Any.OpenApiString("/api/transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"))
                    }
                }
            }
        });

        operation.Responses.TryAdd("409", new OpenApiResponse
        {
            Description = "Conflict – resource was modified by another process",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Example = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["statusCode"] = new Microsoft.OpenApi.Any.OpenApiInteger(409),
                        ["message"] = new Microsoft.OpenApi.Any.OpenApiString("The resource was modified by another process. Please refresh and try again."),
                        ["path"] = new Microsoft.OpenApi.Any.OpenApiString("/api/transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"))
                    }
                }
            }
        });

        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Internal Server Error",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Example = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["statusCode"] = new Microsoft.OpenApi.Any.OpenApiInteger(500),
                        ["message"] = new Microsoft.OpenApi.Any.OpenApiString("An unexpected error occurred."),
                        ["path"] = new Microsoft.OpenApi.Any.OpenApiString("/api/transactions"),
                        ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"))
                    }
                }
            }
        });
    }
}
