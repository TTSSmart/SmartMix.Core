using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SmartMix.Core.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;
    
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        
        var (statusCode, title, detail, errorCode) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message, "INVALID_ARGUMENT"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message, "UNAUTHORIZED"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message, "NOT_FOUND"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", exception.Message, "INVALID_OPERATION"),
            BusinessRuleException bre => (StatusCodes.Status422UnprocessableEntity, "Business Rule Violation", bre.Message, bre.ErrorCode),
            NotFoundException nfe => (StatusCodes.Status404NotFound, "Not Found", nfe.Message, "NOT_FOUND"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", 
                _env.IsDevelopment() ? exception.Message : "An unexpected error occurred", "INTERNAL_ERROR")
        };
        
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions = 
            {
                ["errorCode"] = errorCode,
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}