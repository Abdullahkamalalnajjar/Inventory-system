using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using InventoryManagementSystem.Api.Extensions;

namespace InventoryManagementSystem.Api.Infrastructure;

public sealed class GlobalExceptionHandler(IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await httpContext
            .ToProblem(exception, environment.IsDevelopment())
            .ExecuteAsync(httpContext);

        return true;
    }
}
