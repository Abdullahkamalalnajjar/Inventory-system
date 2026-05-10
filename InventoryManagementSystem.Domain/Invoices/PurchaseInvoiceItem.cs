using InventoryManagementSystem.Domain.Common;

namespace InventoryManagementSystem.Domain.Invoices;

public sealed class PurchaseInvoiceItem : Entity
{
    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitCost { get; private set; }

    public decimal Total => Quantity * UnitCost;

    private PurchaseInvoiceItem()
    {
    }

    internal PurchaseInvoiceItem(Guid productId, int quantity, decimal unitCost)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
    }
}
