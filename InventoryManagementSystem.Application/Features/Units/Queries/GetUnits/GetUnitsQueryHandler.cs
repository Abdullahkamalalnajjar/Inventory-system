using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Application.Features.Units.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Units.Queries.GetUnits;

public class GetUnitsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetUnitsQueryHandler> logger)
    : IRequestHandler<GetUnitsQuery, Result<List<UnitDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetUnitsQueryHandler> _logger = logger;

    public async Task<Result<List<UnitDto>>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching units list.");
        var units = await _context.Units.AsNoTracking().ToListAsync(cancellationToken);
        return units.ToUnitDtoList();
    }
}
