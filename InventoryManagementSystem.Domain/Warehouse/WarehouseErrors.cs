using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Warehouse;

public static class WarehouseErrors
{
    public static Error WarehouseNotFound =>
        Error.NotFound("Warehouse_Not_Found", "Warehouse not found.");

    public static Error NameRequired =>
        Error.Validation("Warehouse_Name_Required", "Warehouse name is required.");
}
