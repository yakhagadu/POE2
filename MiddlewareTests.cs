using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDay.API.Middleware;
using Xunit;

namespace RaceDay.Tests;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task Middleware_WithNoException_PassesThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new ExceptionMiddleware(
            next: _ => Task.CompletedTask,
            logger: NullLogger<ExceptionMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200); // untouched
    }

    [Fact]
    public async Task Middleware_WithException_Returns500WithJsonBody()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionMiddleware(
            next: _ => throw new InvalidOperationException("Boom"),
            logger: NullLogger<ExceptionMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400); // InvalidOperationException maps to 400
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var json = JsonDocument.Parse(body);

        json.RootElement.GetProperty("message").GetString()
            .Should().Be("Boom");
    }

    [Fact]
    public async Task Middleware_WithUnknownException_Returns500()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionMiddleware(
            next: _ => throw new Exception("Unknown"),
            logger: NullLogger<ExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Middleware_WithKeyNotFound_Returns404()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionMiddleware(
            next: _ => throw new KeyNotFoundException("Missing"),
            logger: NullLogger<ExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(404);
    }
}