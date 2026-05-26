using InventoryManagementSystem.Domain.Invoices;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;

public record SalesInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid WarehouseId,
    string WarehouseName,
    Guid? CustomerId,
    DateTimeOffset InvoiceDateUtc,
    InvoiceStatus Status,
    decimal Total,
    List<SalesInvoiceItemDto> Items);
