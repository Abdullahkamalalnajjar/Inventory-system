using MediatR;

namespace InventoryManagementSystem.Application.Common.Interfaces;

public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
    IEnumerable<string>? Tags { get; }
}
public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;