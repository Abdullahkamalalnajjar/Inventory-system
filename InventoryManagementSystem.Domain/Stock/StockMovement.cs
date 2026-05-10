using InventoryManagementSystem.Domain.Common;

namespace InventoryManagementSystem.Domain.Stock;

public sealed class StockMovement : AuditableEntity
{
    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public StockMovementType Type { get; private set; }

    public int Quantity { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public string? Notes { get; private set; }

    private StockMovement()
    {
    }

    internal StockMovement(
        Guid productId,
        Guid warehouseId,
        StockMovementType type,
        int quantity,
        string? referenceNumber = null,
        string? notes = null)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        Type = type;
        Quantity = quantity;
        ReferenceNumber = referenceNumber;
        Notes = notes;
    }
}
