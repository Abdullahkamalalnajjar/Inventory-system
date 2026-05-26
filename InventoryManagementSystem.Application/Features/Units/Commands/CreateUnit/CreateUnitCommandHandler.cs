using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Application.Features.Units.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Unit = InventoryManagementSystem.Domain.Product.Unit;

namespace InventoryManagementSystem.Application.Features.Units.Commands.CreateUnitCommand;

public class CreateUnitCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreateUnitCommandHandler> logger)
    : IRequestHandler<CreateUnitCommand, Result<UnitDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreateUnitCommandHandler> _logger = logger;

    public async Task<Result<UnitDto>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var name = request.UnitName.Trim().ToLower();
        var exists = await _context.Units.AnyAsync(x => x.Name.ToLower() == name, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Unit creation aborted. Name: {Name} already exists.", name);
            return ProductErrors.UnitConflict;
        }

        var createResult = Unit.Create(request.UnitName, request.Symbol);
        if (createResult.IsError)
        {
            _logger.LogWarning("Error creating unit: {UnitName}", request.UnitName);
            return createResult.Errors;
        }

        await _context.Units.AddAsync(createResult.Value, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(UnitsCacheKeys.UnitsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(UnitsCacheKeys.UnitsTag, cancellationToken);

        _logger.LogInformation("Unit created successfully. Id: {UnitId}", createResult.Value.Id);
        return createResult.Value.ToUnitDto();
    }
}
