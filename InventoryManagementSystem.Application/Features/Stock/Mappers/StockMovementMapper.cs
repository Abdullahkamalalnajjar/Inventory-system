using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Domain.Stock;

namespace InventoryManagementSystem.Application.Features.Stock.Mappers;

public static class StockMovementMapper
{
    public static StockMovementDto ToStockMovementDto(this StockMovement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);
        return new StockMovementDto(
            movement.Id,
            movement.ProductId,
            movement.Product?.Name ?? string.Empty,
            movement.WarehouseId,
            movement.Warehouse?.Name ?? string.Empty,
            movement.Type,
            movement.Quantity,
            movement.ReferenceNumber,
            movement.Notes,
            movement.CreatedAtUtc);
    }

    public static List<StockMovementDto> ToStockMovementDtoList(this IEnumerable<StockMovement> movements)
    {
        return movements.Select(x => x.ToStockMovementDto()).ToList();
    }
}
