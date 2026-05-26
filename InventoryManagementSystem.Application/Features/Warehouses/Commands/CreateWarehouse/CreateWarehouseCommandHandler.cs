using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Warehouses;
using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Application.Features.Warehouses.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Warehouse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreateWarehouseCommandHandler> logger)
    : IRequestHandler<CreateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreateWarehouseCommandHandler> _logger = logger;

    public async Task<Result<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim().ToLower();
        var exists = await _context.Warehouses.AnyAsync(x => x.Name.ToLower() == name, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Warehouse creation aborted. Name: {Name} already exists.", name);
            return WarehouseErrors.WarehouseConflict;
        }

        var createResult = Warehouse.Create(request.Name, request.Address);
        if (createResult.IsError)
        {
            _logger.LogWarning("Error when creating warehouse: {Name}", request.Name);
            return createResult.Errors;
        }

        await _context.Warehouses.AddAsync(createResult.Value, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(WarehousesCacheKeys.WarehousesListKey, cancellationToken);
        await _cache.RemoveByTagAsync(WarehousesCacheKeys.WarehousesTag, cancellationToken);
        _logger.LogInformation("Warehouse created successfully. Id: {WarehouseId}", createResult.Value.Id);
        return createResult.Value.ToWarehouseDto();
    }
}
