using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products;
using InventoryManagementSystem.Application.Features.Stock;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Stock;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.PostPurchaseInvoice;

public sealed class PostPurchaseInvoiceCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<PostPurchaseInvoiceCommandHandler> logger)
    : IRequestHandler<PostPurchaseInvoiceCommand, Result<Updated>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<PostPurchaseInvoiceCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(PostPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.PurchaseInvoices
            .AsTracking()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.PurchaseInvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Purchase invoice with id {InvoiceId} not found.", request.PurchaseInvoiceId);
            return InvoiceErrors.InvoiceNotFound;
        }

        var postResult = invoice.Post();
        if (postResult.IsError)
        {
            return postResult.Errors;
        }

        foreach (var item in invoice.Items)
        {
            var stockItem = _context.StockItems.Local.FirstOrDefault(
                s => s.ProductId == item.ProductId && s.WarehouseId == invoice.WarehouseId);

            var isNewStockItem = stockItem is null;
            if (isNewStockItem)
            {
                stockItem = await _context.StockItems
                    .FirstOrDefaultAsync(
                        s => s.ProductId == item.ProductId && s.WarehouseId == invoice.WarehouseId,
                        cancellationToken);

                if (stockItem is null)
                {
                    var createStockResult = StockItem.Create(item.ProductId, invoice.WarehouseId);
                    if (createStockResult.IsError)
                    {
                        return createStockResult.Errors;
                    }

                    stockItem = createStockResult.Value;
                    _context.StockItems.Add(stockItem);
                }
            }

            var addResult = stockItem!.AddQuantity(
                item.Quantity,
                StockMovementType.Purchase,
                invoice.InvoiceNumber,
                $"Purchase invoice: {invoice.InvoiceNumber}");

            if (addResult.IsError)
            {
                return addResult.Errors;
            }

            _context.StockMovements.Add(addResult.Value);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(PurchaseInvoicesCacheKeys.PurchaseInvoicesListKey, cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);
        await _cache.RemoveAsync(StockCacheKeys.StockItemsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(StockCacheKeys.StockItemsTag, cancellationToken);
        _logger.LogInformation("Purchase invoice posted. Id: {InvoiceId}", invoice.Id);

        return Result.Updated;
    }
}
