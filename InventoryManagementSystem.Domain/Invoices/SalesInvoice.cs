using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Warehouse;

namespace InventoryManagementSystem.Domain.Invoices;

public sealed class SalesInvoice : AuditableEntity
{
    private readonly List<SalesInvoiceItem> items = [];

    public string InvoiceNumber { get; private set; } = string.Empty;

    public Guid WarehouseId { get; private set; }

    public Warehouse.Warehouse Warehouse { get; private set; } = null!;

    public Guid? CustomerId { get; private set; }

    public DateTimeOffset InvoiceDateUtc { get; private set; }

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    public IReadOnlyCollection<SalesInvoiceItem> Items => items.AsReadOnly();

    public decimal Total => items.Sum(item => item.Total);

    private SalesInvoice()
    {
    }

    private SalesInvoice(string invoiceNumber, Guid warehouseId, Guid? customerId, DateTimeOffset invoiceDateUtc)
    {
        InvoiceNumber = invoiceNumber;
        WarehouseId = warehouseId;
        CustomerId = customerId;
        InvoiceDateUtc = invoiceDateUtc;
    }

    public static Result<SalesInvoice> Create(
        string invoiceNumber,
        Guid warehouseId,
        Guid? customerId = null,
        DateTimeOffset? invoiceDateUtc = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return InvoiceErrors.InvoiceNumberRequired;

        if (warehouseId == Guid.Empty)
            return InvoiceErrors.WarehouseRequired;

        return new SalesInvoice(
            invoiceNumber.Trim(),
            warehouseId,
            customerId,
            invoiceDateUtc ?? DateTimeOffset.UtcNow);
    }

    public Result<Updated> AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != InvoiceStatus.Draft)
            return InvoiceErrors.CannotChangePostedInvoice;

        if (productId == Guid.Empty)
            return InvoiceErrors.ProductRequired;

        if (quantity <= 0)
            return InvoiceErrors.QuantityInvalid;

        if (unitPrice < 0)
            return InvoiceErrors.UnitPriceInvalid;

        items.Add(new SalesInvoiceItem(productId, quantity, unitPrice));

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
