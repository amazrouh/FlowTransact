using System.Diagnostics;

namespace Transactions.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request headers
        string correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                               ?? Guid.NewGuid().ToString();

        // Set correlation ID in response headers
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Add to Activity.Current for distributed tracing
        Activity.Current?.SetTag("correlation.id", correlationId);

        // Store in HttpContext.Items for easy access
        context.Items["CorrelationId"] = correlationId;

        await _next(context);
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}