using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Application.Features.Stock.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Stock.Queries.GetStockMovements;

public sealed class GetStockMovementsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetStockMovementsQueryHandler> logger)
    : IRequestHandler<GetStockMovementsQuery, Result<List<StockMovementDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetStockMovementsQueryHandler> _logger = logger;

    public async Task<Result<List<StockMovementDto>>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching stock movements.");

        var query = _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .AsQueryable();

        if (request.StockItemId.HasValue)
        {
            query = query.Where(m => EF.Property<Guid>(m, "StockItemId") == request.StockItemId.Value);
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(m => m.ProductId == request.ProductId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(m => m.WarehouseId == request.WarehouseId.Value);
        }

        var movements = await query
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return movements.ToStockMovementDtoList();
    }
}
