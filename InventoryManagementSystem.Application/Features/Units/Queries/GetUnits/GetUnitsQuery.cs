using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Units.Queries.GetUnits;

public sealed record GetUnitsQuery() : ICachedQuery<Result<List<UnitDto>>>
{
    public string CacheKey => UnitsCacheKeys.UnitsListKey;

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

    public IEnumerable<string>? Tags => [UnitsCacheKeys.UnitsTag];
}
