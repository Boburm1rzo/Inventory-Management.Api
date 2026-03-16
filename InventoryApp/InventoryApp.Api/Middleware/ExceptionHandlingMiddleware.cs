using InventoryApp.Domain.Exceptions;
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
                "SERVER ERROR! TraceId: {TraceId} | Method: {Method} | Path: {Path} | Error: {ErrorMessage}",
                traceId, context.Request.Method, context.Request.Path, ex.Message);

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId)
    {
        context.Response.ContentType = "application/json";

        var isDevelopment = context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        object response;

        switch (ex)
        {
            case NotFoundException e:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = e.Message,
                    TraceId = traceId
                };
                break;

            case ForbiddenException e:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response = new
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = e.Message,
                    TraceId = traceId
                };
                break;

            case OptimisticLockException e:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response = new
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Error = "optimistic_lock",
                    Message = e.Message,
                    TraceId = traceId
                };
                break;

            case DuplicateCustomIdException e:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response = new
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Error = "duplicate_custom_id",
                    ConflictingId = e.ConflictingId,
                    Message = e.Message,
                    TraceId = traceId
                };
                break;

            case DomainException e:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = e.Message,
                    TraceId = traceId
                };
                break;

            case UnauthorizedAccessException e:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = e.Message,
                    TraceId = traceId
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = isDevelopment ? ex.Message : "Internal server error.",
                    Trace = isDevelopment ? ex.StackTrace : null,
                    TraceId = traceId
                };
                break;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}