using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using InventoryManagementSystem.Domain.Warehouse;

namespace InventoryManagementSystem.Domain.Stock;

public sealed class StockItem : AuditableEntity
{
    private readonly List<StockMovement> movements = [];

    public Guid ProductId { get; private set; }

    public Product.Product Product { get; private set; } = null!;

    public Guid WarehouseId { get; private set; }

    public Warehouse.Warehouse Warehouse { get; private set; } = null!;

    public int QuantityOnHand { get; private set; }

    public IReadOnlyCollection<StockMovement> Movements => movements.AsReadOnly();

    private StockItem()
    {
    }

    private StockItem(Guid productId, Guid warehouseId)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
    }

    public static Result<StockItem> Create(Guid productId, Guid warehouseId)
    {
        if (productId == Guid.Empty)
            return StockErrors.ProductRequired;

        if (warehouseId == Guid.Empty)
            return StockErrors.WarehouseRequired;

        return new StockItem(productId, warehouseId);
    }

    public Result<StockMovement> AddQuantity(
        int quantity,
        StockMovementType type = StockMovementType.Purchase,
        string? referenceNumber = null,
        string? notes = null)
    {
        if (quantity <= 0)
            return StockErrors.QuantityInvalid;

        var movement = new StockMovement(Id, ProductId, WarehouseId, type, quantity, referenceNumber, notes);

        QuantityOnHand += quantity;
        movements.Add(movement);

        return movement;
    }

    public Result<StockMovement> RemoveQuantity(
        int quantity,
        StockMovementType type = StockMovementType.Sale,
        string? referenceNumber = null,
        string? notes = null)
    {
        if (quantity <= 0)
            return StockErrors.QuantityInvalid;

        if (QuantityOnHand < quantity)
            return StockErrors.InsufficientQuantity;

        var movement = new StockMovement(Id, ProductId, WarehouseId, type, quantity, referenceNumber, notes);

        QuantityOnHand -= quantity;
        movements.Add(movement);

        return movement;
    }

    public Result<StockMovement?> AdjustQuantity(int newQuantity, string? referenceNumber = null, string? notes = null)
    {
        if (newQuantity < 0)
            return StockErrors.QuantityInvalid;

        var difference = newQuantity - QuantityOnHand;
        StockMovement? movement = null;

        QuantityOnHand = newQuantity;

        if (difference != 0)
        {
            movement = new StockMovement(
                Id,
                ProductId,
                WarehouseId,
                StockMovementType.Adjustment,
                Math.Abs(difference),
                referenceNumber,
                notes);

            movements.Add(movement);
        }

        return movement;
    }
}
