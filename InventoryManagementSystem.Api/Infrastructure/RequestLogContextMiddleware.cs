namespace InventoryManagementSystem.Api.Infrastructure;

public sealed class RequestLogContextMiddleware(RequestDelegate next, ILogger<RequestLogContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers["X-Correlation-Id"] = httpContext.TraceIdentifier;

        using (logger.BeginScope(new Dictionary<string, object?>
               {
                   ["CorrelationId"] = httpContext.TraceIdentifier
               }))
        {
            await next(httpContext);
        }
    }
}
