using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Warehouses;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Warehouse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<UpdateWarehouseCommandHandler> logger)
    : IRequestHandler<UpdateWarehouseCommand, Result<Updated>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<UpdateWarehouseCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses.FindAsync([request.WarehouseId], cancellationToken);

        if (warehouse is null)
        {
            _logger.LogWarning("Warehouse with id {WarehouseId} not found.", request.WarehouseId);
            return WarehouseErrors.WarehouseNotFound;
        }

        var normalizedName = request.Name.Trim().ToLower();
        var duplicateNameExists = await _context.Warehouses.AnyAsync(
            x => x.Id != request.WarehouseId && x.Name.ToLower() == normalizedName,
            cancellationToken);

        if (duplicateNameExists)
        {
            _logger.LogWarning("Warehouse update aborted. Name: {Name} already exists.", request.Name);
            return WarehouseErrors.WarehouseConflict;
        }

        var updateResult = warehouse.Update(request.Name, request.Address);
        if (updateResult.IsError)
        {
            _logger.LogWarning("Error while updating warehouse with id {WarehouseId}.", request.WarehouseId);
            return updateResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(WarehousesCacheKeys.WarehousesListKey, cancellationToken);
        await _cache.RemoveByTagAsync(WarehousesCacheKeys.WarehousesTag, cancellationToken);

        _logger.LogInformation("Warehouse updated successfully. Id: {WarehouseId}", warehouse.Id);
        return Result.Updated;
    }
}
