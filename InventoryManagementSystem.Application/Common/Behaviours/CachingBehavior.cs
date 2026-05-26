using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Common.Behaviours;

public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(ct);
        }

        _logger.LogInformation("Checking cache for {RequestName}", typeof(TRequest).Name);

        var result = await _cache.GetOrCreateAsync(
            cachedRequest.CacheKey,
            async token => await next(token),
            new HybridCacheEntryOptions
            {
                Expiration = cachedRequest.Expiration
            },
            cachedRequest.Tags,
            cancellationToken: ct);

        if (result is IResult res && !res.IsSuccess)
        {
            await _cache.RemoveAsync(cachedRequest.CacheKey, ct);
        }

        return result;
    }
}
