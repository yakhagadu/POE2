using System.Net;
using System.Text.Json;

namespace RaceDay.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "A required argument was missing."),
            ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authorized to perform this action."),
            InvalidOperationException invEx => (HttpStatusCode.BadRequest, invEx.Message),
            _ => (HttpStatusCode.InternalServerError, "An internal server error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}