using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products;
using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Application.Features.Stock.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using InventoryManagementSystem.Domain.Stock;
using InventoryManagementSystem.Domain.Warehouse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.CreateStockItem;

public sealed class CreateStockItemCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreateStockItemCommandHandler> logger)
    : IRequestHandler<CreateStockItemCommand, Result<StockItemDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreateStockItemCommandHandler> _logger = logger;

    public async Task<Result<StockItemDto>> Handle(CreateStockItemCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning("Product with id {ProductId} not found.", request.ProductId);
            return ProductErrors.ProductNotFound;
        }

        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            _logger.LogWarning("Warehouse with id {WarehouseId} not found.", request.WarehouseId);
            return WarehouseErrors.WarehouseNotFound;
        }

        var exists = await _context.StockItems
            .AsNoTracking()
            .AnyAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Stock item for product {ProductId} and warehouse {WarehouseId} already exists.", request.ProductId, request.WarehouseId);
            return StockErrors.StockItemAlreadyExists;
        }

        var createResult = StockItem.Create(request.ProductId, request.WarehouseId);
        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        var stockItem = createResult.Value;
        _context.StockItems.Add(stockItem);

        if (request.InitialQuantity > 0)
        {
            var adjustResult = stockItem.AdjustQuantity(request.InitialQuantity, notes: "Initial stock");
            if (adjustResult.IsError)
            {
                return adjustResult.Errors;
            }

            if (adjustResult.Value is not null)
            {
                _context.StockMovements.Add(adjustResult.Value);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);
        await _cache.RemoveAsync(StockCacheKeys.StockItemsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(StockCacheKeys.StockItemsTag, cancellationToken);
        _logger.LogInformation("Stock item created successfully. Id: {StockItemId}", stockItem.Id);

        return stockItem.ToStockItemDto();
    }
}
