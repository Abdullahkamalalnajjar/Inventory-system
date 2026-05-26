using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Domain.Invoices;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Mappers;

public static class SalesInvoiceMapper
{
    public static SalesInvoiceDto ToSalesInvoiceDto(this SalesInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        return new SalesInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.WarehouseId,
            invoice.Warehouse?.Name ?? string.Empty,
            invoice.CustomerId,
            invoice.InvoiceDateUtc,
            invoice.Status,
            invoice.Total,
            invoice.Items.Select(i => i.ToSalesInvoiceItemDto()).ToList());
    }

    public static SalesInvoiceItemDto ToSalesInvoiceItemDto(this SalesInvoiceItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new SalesInvoiceItemDto(
            item.Id,
            item.ProductId,
            item.Product?.Name ?? string.Empty,
            item.Quantity,
            item.UnitPrice,
            item.Total);
    }

    public static List<SalesInvoiceDto> ToSalesInvoiceDtoList(this IEnumerable<SalesInvoice> invoices)
    {
        return invoices.Select(i => i.ToSalesInvoiceDto()).ToList();
    }
}
