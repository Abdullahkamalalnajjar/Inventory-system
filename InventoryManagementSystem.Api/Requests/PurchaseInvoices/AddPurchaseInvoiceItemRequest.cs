namespace InventoryManagementSystem.Api.Requests.PurchaseInvoices;

public sealed record AddPurchaseInvoiceItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitCost);
