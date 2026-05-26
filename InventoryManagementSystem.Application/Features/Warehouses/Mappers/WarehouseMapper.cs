using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Domain.Warehouse;

namespace InventoryManagementSystem.Application.Features.Warehouses.Mappers;

public static class WarehouseMapper
{
    public static WarehouseDto ToWarehouseDto(this Warehouse warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);
        return new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Address);
    }

    public static List<WarehouseDto> ToWarehouseDtoList(this IEnumerable<Warehouse> warehouses)
    {
        return warehouses.Select(x => x.ToWarehouseDto()).ToList();
    }
}
