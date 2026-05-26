using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Warehouses.Queries.GetWarehouses;

public sealed record GetWarehousesQuery() : ICachedQuery<Result<List<WarehouseDto>>>
{
    public string CacheKey => WarehousesCacheKeys.WarehousesListKey;

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

    public IEnumerable<string>? Tags => [WarehousesCacheKeys.WarehousesTag];
}
