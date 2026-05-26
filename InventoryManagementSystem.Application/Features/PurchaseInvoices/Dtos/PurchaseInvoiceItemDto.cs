namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;

public record PurchaseInvoiceItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitCost,
    decimal Total);
