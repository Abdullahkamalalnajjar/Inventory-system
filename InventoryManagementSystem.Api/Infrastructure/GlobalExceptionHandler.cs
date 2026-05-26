using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using InventoryManagementSystem.Api.Extensions;

namespace InventoryManagementSystem.Api.Infrastructure;

public sealed class GlobalExceptionHandler(
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}. TraceIdentifier: {TraceIdentifier}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        await httpContext
            .ToProblem(exception, environment.IsDevelopment())
            .ExecuteAsync(httpContext);

        return true;
    }
}
