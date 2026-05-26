using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Domain.Product;

namespace InventoryManagementSystem.Application.Features.Units.Mappers;

public static class UnitMapper
{
    public static UnitDto ToUnitDto(this Unit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        return new UnitDto(unit.Id,unit.Name, unit.Symbol);
    }

    public static List<UnitDto> ToUnitDtoList(this IEnumerable<Unit> units)
    {
        return units.Select(x => x.ToUnitDto()).ToList();
    }
}
