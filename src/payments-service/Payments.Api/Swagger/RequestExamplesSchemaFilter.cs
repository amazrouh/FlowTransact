using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Payments.Api.DTOs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Payments.Api.Swagger;

/// <summary>
/// Adds example values to request schemas for better Swagger UI experience.
/// </summary>
public class RequestExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(StartPaymentRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["transactionId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["customerId"] = new OpenApiString("7c9e6679-7425-40de-944b-e07fc1f90ae7")
            };
        }
        else if (context.Type == typeof(FailPaymentRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["reason"] = new OpenApiString("Insufficient funds")
            };
        }
    }
}
