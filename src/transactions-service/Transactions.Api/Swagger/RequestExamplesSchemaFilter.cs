using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Transactions.Api.DTOs;

namespace Transactions.Api.Swagger;

/// <summary>
/// Adds example values to request schemas for better Swagger UI experience.
/// </summary>
public class RequestExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreateTransactionRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["customerId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
            };
        }
        else if (context.Type == typeof(AddItemRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["productId"] = new OpenApiString("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                ["productName"] = new OpenApiString("Premium Subscription"),
                ["quantity"] = new OpenApiInteger(2),
                ["unitPrice"] = new OpenApiDouble(29.99)
            };
        }
    }
}
