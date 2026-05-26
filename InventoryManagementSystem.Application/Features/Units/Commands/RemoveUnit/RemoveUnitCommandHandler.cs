using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Units.Commands.RemoveUnit;

public sealed class RemoveUnitCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<RemoveUnitCommandHandler> logger)
    : IRequestHandler<RemoveUnitCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<RemoveUnitCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(RemoveUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units.FindAsync([request.UnitId], cancellationToken);
        if (unit is null)
        {
            _logger.LogWarning("Unit with id {UnitId} not found.", request.UnitId);
            return ProductErrors.UnitNotFound;
        }

        var hasProducts = await _context.Products.AnyAsync(
            product => product.UnitId == request.UnitId,
            cancellationToken);

        if (hasProducts)
        {
            _logger.LogWarning(
                "Unit with id {UnitId} cannot be deleted because it has associated products.",
                request.UnitId);
            return ProductErrors.CannotDeleteUnitWithProducts;
        }

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(UnitsCacheKeys.UnitsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(UnitsCacheKeys.UnitsTag, cancellationToken);

        _logger.LogInformation("Unit with id {UnitId} deleted successfully.", request.UnitId);
        return Result.Deleted;
    }
}
