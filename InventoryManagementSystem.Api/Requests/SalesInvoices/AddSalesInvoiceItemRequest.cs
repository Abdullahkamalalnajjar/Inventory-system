namespace InventoryManagementSystem.Api.Requests.SalesInvoices;

public sealed record AddSalesInvoiceItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);
