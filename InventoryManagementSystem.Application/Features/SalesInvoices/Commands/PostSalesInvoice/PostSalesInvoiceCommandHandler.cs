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

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.PostSalesInvoice;

public sealed class PostSalesInvoiceCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<PostSalesInvoiceCommandHandler> logger)
    : IRequestHandler<PostSalesInvoiceCommand, Result<Updated>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<PostSalesInvoiceCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(PostSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.SalesInvoices
            .AsTracking()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.SalesInvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Sales invoice with id {InvoiceId} not found.", request.SalesInvoiceId);
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

            if (stockItem is null)
            {
                stockItem = await _context.StockItems
                    .FirstOrDefaultAsync(
                        s => s.ProductId == item.ProductId && s.WarehouseId == invoice.WarehouseId,
                        cancellationToken);
            }

            if (stockItem is null)
            {
                _logger.LogWarning(
                    "No stock item found for product {ProductId} in warehouse {WarehouseId}.",
                    item.ProductId, invoice.WarehouseId);
                return StockErrors.StockItemNotFound;
            }

            var removeResult = stockItem.RemoveQuantity(
                item.Quantity,
                StockMovementType.Sale,
                invoice.InvoiceNumber,
                $"Sales invoice: {invoice.InvoiceNumber}");

            if (removeResult.IsError)
            {
                return removeResult.Errors;
            }

            _context.StockMovements.Add(removeResult.Value);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(SalesInvoicesCacheKeys.SalesInvoicesListKey, cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);
        await _cache.RemoveAsync(StockCacheKeys.StockItemsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(StockCacheKeys.StockItemsTag, cancellationToken);
        _logger.LogInformation("Sales invoice posted. Id: {InvoiceId}", invoice.Id);

        return Result.Updated;
    }
}
