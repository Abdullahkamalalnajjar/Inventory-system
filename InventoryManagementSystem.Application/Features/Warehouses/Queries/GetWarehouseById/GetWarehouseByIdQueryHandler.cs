using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Application.Features.Warehouses.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Warehouse;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Warehouses.Queries.GetWarehouseById;

public class GetWarehouseByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetWarehouseByIdQueryHandler> logger)
    : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetWarehouseByIdQueryHandler> _logger = logger;

    public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses.FindAsync([request.Id], cancellationToken);
        if (warehouse is null)
        {
            _logger.LogError("Warehouse with id {WarehouseId} not found", request.Id);
            return WarehouseErrors.WarehouseNotFound;
        }
        return warehouse.ToWarehouseDto();
    }
}
