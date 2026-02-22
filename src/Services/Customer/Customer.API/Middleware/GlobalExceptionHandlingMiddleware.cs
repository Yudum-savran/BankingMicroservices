using System.Net;
using System.Text.Json;
using Customer.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AppValidationException = Customer.Domain.Exceptions.ValidationException;
using AppInvalidOperationException = Customer.Domain.Exceptions.InvalidOperationException;

namespace Customer.API.Middleware;

/// <summary>
/// Global error handling middleware
/// Catches exceptions and returns consistent error responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();
        var statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case AppValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                response = new ErrorResponse
                {
                    Code = validationEx.Code,
                    Message = validationEx.Message,
                    Errors = validationEx.Errors,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Validation error: {Errors}", 
                    JsonSerializer.Serialize(validationEx.Errors));
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                response = new ErrorResponse
                {
                    Code = notFoundEx.Code,
                    Message = notFoundEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);
                break;

            case DuplicateException duplicateEx:
                statusCode = HttpStatusCode.Conflict;
                response = new ErrorResponse
                {
                    Code = duplicateEx.Code,
                    Message = duplicateEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Duplicate resource: {Message}", duplicateEx.Message);
                break;

            case DomainException domainEx:
                statusCode = HttpStatusCode.BadRequest;
                response = new ErrorResponse
                {
                    Code = domainEx.Code,
                    Message = domainEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Domain exception: {Code} - {Message}", 
                    domainEx.Code, domainEx.Message);
                break;

            case AppInvalidOperationException invalidOpEx:
                statusCode = HttpStatusCode.BadRequest;
                response = new ErrorResponse
                {
                    Code = invalidOpEx.Code,
                    Message = invalidOpEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Invalid operation: {Message}", invalidOpEx.Message);
                break;

            case UnauthorizedException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                response = new ErrorResponse
                {
                    Code = unauthorizedEx.Code,
                    Message = unauthorizedEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Unauthorized access: {Message}", unauthorizedEx.Message);
                break;

            case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                response = new ErrorResponse
                {
                    Code = forbiddenEx.Code,
                    Message = forbiddenEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("Forbidden access: {Message}", forbiddenEx.Message);
                break;

            case FluentValidation.ValidationException fluentEx:
                statusCode = HttpStatusCode.BadRequest;
                var errors = fluentEx.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray());

                response = new ErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred",
                    Errors = errors,
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogWarning("FluentValidation error: {Errors}", 
                    JsonSerializer.Serialize(errors));
                break;

            default:
                response = new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred. Please try again later.",
                    Timestamp = DateTime.UtcNow
                };
                _logger.LogError("Unhandled exception type {Type}: {Message}", 
                    exception.GetType().Name, exception.Message);
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Standard error response model
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Machine-readable error code
    /// </summary>
    public string Code { get; set; } = "UNKNOWN_ERROR";

    /// <summary>
    /// Human-readable error message (safe to expose to client)
    /// </summary>
    public string Message { get; set; } = "An error occurred";

    /// <summary>
    /// Validation errors grouped by property name
    /// Only populated for validation errors
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp when error occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Trace ID for debugging (from HttpContext if available)
    /// </summary>
    public string? TraceId { get; set; }
}
