using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Product;

namespace InventoryManagementSystem.Domain.Invoices;

public sealed class PurchaseInvoiceItem : Entity
{
    public Guid ProductId { get; private set; }

    public Product.Product Product { get; private set; } = null!;

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
