using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Warehouse;

namespace InventoryManagementSystem.Domain.Invoices;

public sealed class PurchaseInvoice : AuditableEntity
{
    private readonly List<PurchaseInvoiceItem> items = [];

    public string InvoiceNumber { get; private set; } = string.Empty;

    public Guid WarehouseId { get; private set; }

    public Warehouse.Warehouse Warehouse { get; private set; } = null!;

    public Guid? SupplierId { get; private set; }

    public DateTimeOffset InvoiceDateUtc { get; private set; }

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    public IReadOnlyCollection<PurchaseInvoiceItem> Items => items.AsReadOnly();

    public decimal Total => items.Sum(item => item.Total);

    private PurchaseInvoice()
    {
    }

    private PurchaseInvoice(string invoiceNumber, Guid warehouseId, Guid? supplierId, DateTimeOffset invoiceDateUtc)
    {
        InvoiceNumber = invoiceNumber;
        WarehouseId = warehouseId;
        SupplierId = supplierId;
        InvoiceDateUtc = invoiceDateUtc;
    }

    public static Result<PurchaseInvoice> Create(
        string invoiceNumber,
        Guid warehouseId,
        Guid? supplierId = null,
        DateTimeOffset? invoiceDateUtc = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return InvoiceErrors.InvoiceNumberRequired;

        if (warehouseId == Guid.Empty)
            return InvoiceErrors.WarehouseRequired;

        return new PurchaseInvoice(
            invoiceNumber.Trim(),
            warehouseId,
            supplierId,
            invoiceDateUtc ?? DateTimeOffset.UtcNow);
    }

    public Result<Updated> AddItem(Guid productId, int quantity, decimal unitCost)
    {
        if (Status != InvoiceStatus.Draft)
            return InvoiceErrors.CannotChangePostedInvoice;

        if (productId == Guid.Empty)
            return InvoiceErrors.ProductRequired;

        if (quantity <= 0)
            return InvoiceErrors.QuantityInvalid;

        if (unitCost < 0)
            return InvoiceErrors.UnitPriceInvalid;

        items.Add(new PurchaseInvoiceItem(productId, quantity, unitCost));

        return Result.Updated;
    }

    public Result<Deleted> RemoveItem(Guid itemId)
    {
        if (Status != InvoiceStatus.Draft)
            return InvoiceErrors.CannotChangePostedInvoice;

        var item = items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return InvoiceErrors.InvoiceItemNotFound;

        items.Remove(item);

        return Result.Deleted;
    }

    public Result<Updated> Post()
    {
        if (Status == InvoiceStatus.Posted)
            return InvoiceErrors.AlreadyPosted;

        if (items.Count == 0)
            return InvoiceErrors.EmptyInvoice;

        Status = InvoiceStatus.Posted;

        return Result.Updated;
    }
}
