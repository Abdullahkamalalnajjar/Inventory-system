using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Application.Features.Warehouses.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Warehouses.Queries.GetWarehouses;

public class GetWarehousesQueryHandler(
    IApplicationDbContext context,
    ILogger<GetWarehousesQueryHandler> logger)
    : IRequestHandler<GetWarehousesQuery, Result<List<WarehouseDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetWarehousesQueryHandler> _logger = logger;

    public async Task<Result<List<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching warehouses list.");
        var warehouses = await _context.Warehouses.AsNoTracking().ToListAsync(cancellationToken);
        return warehouses.ToWarehouseDtoList();
    }
}
