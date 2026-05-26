using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Product;
using InventoryManagementSystem.Domain.Warehouse;

namespace InventoryManagementSystem.Domain.Stock;

public sealed class StockMovement : AuditableEntity
{
    public Guid StockItemId { get; private set; }

    public Guid ProductId { get; private set; }

    public Product.Product Product { get; private set; } = null!;

    public Guid WarehouseId { get; private set; }

    public Warehouse.Warehouse Warehouse { get; private set; } = null!;

    public StockMovementType Type { get; private set; }

    public int Quantity { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public string? Notes { get; private set; }

    private StockMovement()
    {
    }

    internal StockMovement(
        Guid stockItemId,
        Guid productId,
        Guid warehouseId,
        StockMovementType type,
        int quantity,
        string? referenceNumber = null,
        string? notes = null)
    {
        StockItemId = stockItemId;
        ProductId = productId;
        WarehouseId = warehouseId;
        Type = type;
        Quantity = quantity;
        ReferenceNumber = referenceNumber;
        Notes = notes;
    }
}
