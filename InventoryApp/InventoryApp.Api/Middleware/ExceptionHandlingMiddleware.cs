using InventoryApp.Domain.Extentions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace InventoryApp.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            var traceId = context.TraceIdentifier;

            _logger.LogError(ex,
                "🚨 SERVER ERROR! TraceId: {TraceId} | Method: {Method} | Path: {Path} | Error: {ErrorMessage}",
                traceId, context.Request.Method, context.Request.Path, ex.Message);

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = ex switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            DbUpdateConcurrencyException => (int)HttpStatusCode.Conflict,
            OptimisticLockException => (int)HttpStatusCode.Conflict,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            NotFoundException => (int)HttpStatusCode.NotFound,
            DuplicateCustomIdExtention => (int)HttpStatusCode.Conflict,
            DomainException => (int)HttpStatusCode.InternalServerError,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = ex.Message,
            TraceId = traceId
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}