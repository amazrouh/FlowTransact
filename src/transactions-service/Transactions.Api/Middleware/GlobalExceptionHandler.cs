using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Transactions.Api.Middleware;

public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = GetErrorMessage(exception),
            Path = context.Request.Path,
            Timestamp = DateTime.UtcNow,
            Errors = GetValidationErrors(exception)
        };

        // Get correlation ID for tracing
        var correlationId = context.Items["CorrelationId"] as string ?? "unknown";

        // Log the exception with structured data
        _logger.LogError(exception,
            "Unhandled exception occurred. Path: {Path}, StatusCode: {StatusCode}, CorrelationId: {CorrelationId}, UserAgent: {UserAgent}",
            context.Request.Path, statusCode, correlationId, context.Request.Headers.UserAgent);

        // Set response
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => (int)HttpStatusCode.BadRequest,
        ArgumentException => (int)HttpStatusCode.BadRequest,
        InvalidOperationException => (int)HttpStatusCode.BadRequest,
        KeyNotFoundException => (int)HttpStatusCode.NotFound,
        DbUpdateConcurrencyException => (int)HttpStatusCode.Conflict,
        _ => (int)HttpStatusCode.InternalServerError
    };

    private static string GetErrorMessage(Exception exception) => exception switch
    {
        ValidationException => "One or more validation errors occurred.",
        DbUpdateConcurrencyException => "The resource was modified by another process. Please refresh and try again.",
        _ => exception.Message
    };

    private static Dictionary<string, string[]>? GetValidationErrors(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            return validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }
        return null;
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}