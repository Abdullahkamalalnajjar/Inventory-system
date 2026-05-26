namespace InventoryManagementSystem.Api.Requests.StockItems;

public sealed record CreateStockItemRequest(
    Guid ProductId,
    Guid WarehouseId,
    int InitialQuantity);
