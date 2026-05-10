using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Stock;

public static class StockErrors
{
    public static Error ProductRequired =>
        Error.Validation("Stock_Product_Required", "Product is required.");

    public static Error WarehouseRequired =>
        Error.Validation("Stock_Warehouse_Required", "Warehouse is required.");

    public static Error QuantityInvalid =>
        Error.Validation("Stock_Quantity_Invalid", "Quantity must be greater than zero.");

    public static Error InsufficientQuantity =>
        Error.Conflict("Stock_Insufficient_Quantity", "There is not enough quantity in stock.");
}
