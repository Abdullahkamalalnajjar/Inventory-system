using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Product;

namespace InventoryManagementSystem.Domain.Invoices;

public sealed class SalesInvoiceItem : Entity
{
    public Guid ProductId { get; private set; }

    public Product.Product Product { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal Total => Quantity * UnitPrice;

    private SalesInvoiceItem()
    {
    }

    internal SalesInvoiceItem(Guid productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
