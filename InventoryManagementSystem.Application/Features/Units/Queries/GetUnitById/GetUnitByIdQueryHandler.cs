using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Application.Features.Units.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Units.Queries.GetUnitById;

public class GetUnitByIdQueryHandler(IApplicationDbContext context, ILogger<GetUnitByIdQueryHandler> logger)
    : IRequestHandler<GetUnitByIdQuery, Result<UnitDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetUnitByIdQueryHandler> _logger = logger;

    public async Task<Result<UnitDto>> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units.FindAsync([request.Id], cancellationToken);
        if (unit is null)
        {
            _logger.LogError("Unit with id {UnitId} not found", request.Id);
            return ProductErrors.UnitNotFound;
        }
        return unit.ToUnitDto();
    }
}
