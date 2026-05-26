namespace InventoryManagementSystem.Api.Requests.StockItems;

public sealed record UpdateStockItemRequest(
    int NewQuantity,
    string? ReferenceNumber = null,
    string? Notes = null);
