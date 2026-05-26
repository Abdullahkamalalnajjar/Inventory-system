using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Application.Features.Stock.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Stock.Queries.GetStockItems;

public sealed class GetStockItemsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetStockItemsQueryHandler> logger)
    : IRequestHandler<GetStockItemsQuery, Result<List<StockItemDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetStockItemsQueryHandler> _logger = logger;

    public async Task<Result<List<StockItemDto>>> Handle(GetStockItemsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching stock items list.");
        var query = _context.StockItems
            .AsNoTracking()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(s => s.ProductId == request.ProductId.Value);
        }

        var stockItems = await query.ToListAsync(cancellationToken);
        return stockItems.ToStockItemDtoList();
    }
}
