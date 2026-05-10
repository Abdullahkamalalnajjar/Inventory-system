using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Stock;

public sealed class StockItem : AuditableEntity
{
    private readonly List<StockMovement> movements = [];

    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

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

    public Result<Updated> AddQuantity(
        int quantity,
        StockMovementType type = StockMovementType.Purchase,
        string? referenceNumber = null,
        string? notes = null)
    {
        if (quantity <= 0)
            return StockErrors.QuantityInvalid;

        QuantityOnHand += quantity;
        movements.Add(new StockMovement(ProductId, WarehouseId, type, quantity, referenceNumber, notes));

        return Result.Updated;
    }

    public Result<Updated> RemoveQuantity(
        int quantity,
        StockMovementType type = StockMovementType.Sale,
        string? referenceNumber = null,
        string? notes = null)
    {
        if (quantity <= 0)
            return StockErrors.QuantityInvalid;

        if (QuantityOnHand < quantity)
            return StockErrors.InsufficientQuantity;

        QuantityOnHand -= quantity;
        movements.Add(new StockMovement(ProductId, WarehouseId, type, quantity, referenceNumber, notes));

        return Result.Updated;
    }

    public Result<Updated> AdjustQuantity(int newQuantity, string? referenceNumber = null, string? notes = null)
    {
        if (newQuantity < 0)
            return StockErrors.QuantityInvalid;

        var difference = newQuantity - QuantityOnHand;
        QuantityOnHand = newQuantity;

        if (difference != 0)
        {
            movements.Add(new StockMovement(
                ProductId,
                WarehouseId,
                StockMovementType.Adjustment,
                Math.Abs(difference),
                referenceNumber,
                notes));
        }

        return Result.Updated;
    }
}
