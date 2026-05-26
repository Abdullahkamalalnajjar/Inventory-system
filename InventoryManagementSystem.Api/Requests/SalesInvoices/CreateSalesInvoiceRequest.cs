namespace InventoryManagementSystem.Api.Requests.SalesInvoices;

public sealed record CreateSalesInvoiceRequest(
    string InvoiceNumber,
    Guid WarehouseId,
    Guid? CustomerId,
    DateTimeOffset? InvoiceDateUtc);
