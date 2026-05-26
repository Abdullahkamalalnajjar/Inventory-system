namespace InventoryManagementSystem.Api.Requests.PurchaseInvoices;

public sealed record CreatePurchaseInvoiceRequest(
    string InvoiceNumber,
    Guid WarehouseId,
    Guid? SupplierId,
    DateTimeOffset? InvoiceDateUtc);
