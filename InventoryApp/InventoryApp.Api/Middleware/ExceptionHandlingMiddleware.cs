using Newtonsoft.Json;
using System.Net;

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
                 "Unhandled exception. TraceId={TraceId} Method={Method} Path={Path}",
                 traceId, context.Request.Method, context.Request.Path);

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
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = ex.Message,
            traceId
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
