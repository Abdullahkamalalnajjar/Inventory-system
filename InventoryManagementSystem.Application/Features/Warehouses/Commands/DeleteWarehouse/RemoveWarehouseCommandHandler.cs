using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Warehouses;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Warehouse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.DeleteWarehouse;

public class RemoveWarehouseCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<RemoveWarehouseCommandHandler> logger)
    : IRequestHandler<RemoveWarehouseCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<RemoveWarehouseCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(RemoveWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses.FindAsync([request.WarehouseId], cancellationToken);
        if (warehouse is null)
        {
            _logger.LogWarning("Warehouse with id {WarehouseId} not found.", request.WarehouseId);
            return WarehouseErrors.WarehouseNotFound;
        }

        var hasStockItems = await _context.StockItems.AnyAsync(
            x => x.WarehouseId == request.WarehouseId,
            cancellationToken);

        if (hasStockItems)
        {
            _logger.LogWarning(
                "Warehouse with id {WarehouseId} cannot be deleted because it has associated stock items.",
                request.WarehouseId);
            return WarehouseErrors.CannotDeleteWarehouseWithStockItems;
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(WarehousesCacheKeys.WarehousesListKey, cancellationToken);
        await _cache.RemoveByTagAsync(WarehousesCacheKeys.WarehousesTag, cancellationToken);

        return Result.Deleted;
    }
}
