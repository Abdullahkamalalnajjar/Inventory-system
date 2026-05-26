using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Units.Commands.UpdateUnit;

public sealed class UpdateUnitCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<UpdateUnitCommandHandler> logger)
    : IRequestHandler<UpdateUnitCommand, Result<Updated>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<UpdateUnitCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units.FindAsync([request.UnitId], cancellationToken);

        if (unit is null)
        {
            _logger.LogWarning("Unit with id {UnitId} not found.", request.UnitId);
            return ProductErrors.UnitNotFound;
        }

        var normalizedName = request.UnitName.Trim().ToLower();
        var duplicateNameExists = await _context.Units.AnyAsync(
            x => x.Id != request.UnitId && x.Name.ToLower() == normalizedName,
            cancellationToken);

        if (duplicateNameExists)
        {
            _logger.LogWarning("Unit update aborted. Name: {Name} already exists.", request.UnitName);
            return ProductErrors.UnitConflict;
        }

        var updateResult = unit.Update(request.UnitName, request.Symbol);
        if (updateResult.IsError)
        {
            _logger.LogWarning("Error while updating unit with id {UnitId}.", request.UnitId);
            return updateResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(UnitsCacheKeys.UnitsListKey, cancellationToken);
        await _cache.RemoveByTagAsync(UnitsCacheKeys.UnitsTag, cancellationToken);

        _logger.LogInformation("Unit updated successfully. Id: {UnitId}", unit.Id);
        return Result.Updated;
    }
}
