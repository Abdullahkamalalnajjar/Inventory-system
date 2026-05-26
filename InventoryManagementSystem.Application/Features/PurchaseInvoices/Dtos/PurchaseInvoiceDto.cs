using InventoryManagementSystem.Domain.Invoices;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;

public record PurchaseInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid WarehouseId,
    string WarehouseName,
    Guid? SupplierId,
    DateTimeOffset InvoiceDateUtc,
    InvoiceStatus Status,
    decimal Total,
    List<PurchaseInvoiceItemDto> Items);
