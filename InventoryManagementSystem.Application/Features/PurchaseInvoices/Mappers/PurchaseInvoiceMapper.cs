using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Domain.Invoices;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Mappers;

public static class PurchaseInvoiceMapper
{
    public static PurchaseInvoiceDto ToPurchaseInvoiceDto(this PurchaseInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        return new PurchaseInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.WarehouseId,
            invoice.Warehouse?.Name ?? string.Empty,
            invoice.SupplierId,
            invoice.InvoiceDateUtc,
            invoice.Status,
            invoice.Total,
            invoice.Items.Select(i => i.ToPurchaseInvoiceItemDto()).ToList());
    }

    public static PurchaseInvoiceItemDto ToPurchaseInvoiceItemDto(this PurchaseInvoiceItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new PurchaseInvoiceItemDto(
            item.Id,
            item.ProductId,
            item.Product?.Name ?? string.Empty,
            item.Quantity,
            item.UnitCost,
            item.Total);
    }

    public static List<PurchaseInvoiceDto> ToPurchaseInvoiceDtoList(this IEnumerable<PurchaseInvoice> invoices)
    {
        return invoices.Select(i => i.ToPurchaseInvoiceDto()).ToList();
    }
}
