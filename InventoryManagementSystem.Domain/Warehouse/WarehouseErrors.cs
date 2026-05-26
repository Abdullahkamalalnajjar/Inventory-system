using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Warehouse;

public static class WarehouseErrors
{
    public static Error WarehouseNotFound =>
        Error.NotFound("Warehouse_Not_Found", "Warehouse not found.");

    public static Error NameRequired =>
        Error.Validation("Warehouse_Name_Required", "Warehouse name is required.");

    public static Error WarehouseConflict =>
        Error.Conflict("Warehouse_Conflict", "Warehouse name already exists.");

    public static Error CannotDeleteWarehouseWithStockItems =>
        Error.Conflict("Warehouse_CannotDelete", "Cannot delete warehouse with associated stock items.");
}
