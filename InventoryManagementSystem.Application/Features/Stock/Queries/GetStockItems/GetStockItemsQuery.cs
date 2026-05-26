using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Stock.Queries.GetStockItems;

public sealed record GetStockItemsQuery(
    Guid? WarehouseId = null,
    Guid? ProductId = null
) : ICachedQuery<Result<List<StockItemDto>>>
{
    public string CacheKey => $"{StockCacheKeys.StockItemsListKey}:w={WarehouseId}:p={ProductId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IEnumerable<string>? Tags => [StockCacheKeys.StockItemsTag];
}
