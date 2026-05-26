namespace InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;

public record SalesInvoiceItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Total);
