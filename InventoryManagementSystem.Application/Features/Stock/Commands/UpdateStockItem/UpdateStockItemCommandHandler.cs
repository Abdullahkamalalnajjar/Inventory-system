using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products;
using InventoryManagementSystem.Application.Features.Stock;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Stock;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.UpdateStockItem;

public sealed class UpdateStockItemCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<UpdateStockItemCommandHandler> logger)
    : IRequestHandler<UpdateStockItemCommand, Result<Updated>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<UpdateStockItemCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(UpdateStockItemCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == request.StockItemId, cancellationToken);

        if (stockItem is null)
        {
            _logger.LogWarning("Stock item with id {StockItemId} not found.", request.StockItemId);
            return StockErrors.StockItemNotFound;
        }

        var adjustResult = stockItem.AdjustQuantity(request.NewQuantity, request.ReferenceNumber, request.Notes);
        if (adjustResult.IsError)
        {
            return adjustResult.Errors;
        }

        if (adjustResult.Value is not null)
        {
            _context.StockMovements.Add(adjustResult.Value);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);
        await _cache.RemoveAsync(StockCacheKeys.StockItemsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(StockCacheKeys.StockItemsTag, cancellationToken);
        _logger.LogInformation("Stock item quantity adjusted. Id: {StockItemId}, NewQuantity: {NewQuantity}", stockItem.Id, request.NewQuantity);

        return Result.Updated;
    }
}
