namespace InventoryManagementSystem.Application.Features.Stock.Dtos;

public record StockItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    int QuantityOnHand);
