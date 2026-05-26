using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.AddPurchaseInvoiceItem;

public sealed class AddPurchaseInvoiceItemCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<AddPurchaseInvoiceItemCommandHandler> logger)
    : IRequestHandler<AddPurchaseInvoiceItemCommand, Result<PurchaseInvoiceItemDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<AddPurchaseInvoiceItemCommandHandler> _logger = logger;

    public async Task<Result<PurchaseInvoiceItemDto>> Handle(AddPurchaseInvoiceItemCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.PurchaseInvoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.PurchaseInvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Purchase invoice with id {InvoiceId} not found.", request.PurchaseInvoiceId);
            return InvoiceErrors.InvoiceNotFound;
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning("Product with id {ProductId} not found.", request.ProductId);
            return ProductErrors.ProductNotFound;
        }

        var existingIds = invoice.Items.Select(i => i.Id).ToHashSet();
        var addResult = invoice.AddItem(request.ProductId, request.Quantity, request.UnitCost);
        if (addResult.IsError)
        {
            return addResult.Errors;
        }

        var newItem = invoice.Items.FirstOrDefault(i => !existingIds.Contains(i.Id));
        if (newItem is not null)
        {
            _context.PurchaseInvoiceItems.Add(newItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(PurchaseInvoicesCacheKeys.PurchaseInvoicesListKey, cancellationToken);
        _logger.LogInformation("Item added to purchase invoice {InvoiceId}.", invoice.Id);

        return newItem?.ToPurchaseInvoiceItemDto() ?? invoice.Items.Last().ToPurchaseInvoiceItemDto();
    }
}
