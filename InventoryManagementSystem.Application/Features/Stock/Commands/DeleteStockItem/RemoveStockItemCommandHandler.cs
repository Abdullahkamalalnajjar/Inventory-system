using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products;
using InventoryManagementSystem.Application.Features.Stock;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Stock;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.DeleteStockItem;

public sealed class RemoveStockItemCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<RemoveStockItemCommandHandler> logger)
    : IRequestHandler<RemoveStockItemCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<RemoveStockItemCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(RemoveStockItemCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == request.StockItemId, cancellationToken);

        if (stockItem is null)
        {
            _logger.LogWarning("Stock item with id {StockItemId} not found.", request.StockItemId);
            return StockErrors.StockItemNotFound;
        }

        _context.StockItems.Remove(stockItem);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);
        await _cache.RemoveAsync(StockCacheKeys.StockItemsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(StockCacheKeys.StockItemsTag, cancellationToken);
        _logger.LogInformation("Stock item removed. Id: {StockItemId}", request.StockItemId);

        return Result.Deleted;
    }
}
