using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Application.Features.Stock.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Stock;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Stock.Queries.GetStockItemById;

public sealed class GetStockItemByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetStockItemByIdQueryHandler> logger)
    : IRequestHandler<GetStockItemByIdQuery, Result<StockItemDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetStockItemByIdQueryHandler> _logger = logger;

    public async Task<Result<StockItemDto>> Handle(GetStockItemByIdQuery request, CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .AsNoTracking()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (stockItem is null)
        {
            _logger.LogWarning("Stock item with id {StockItemId} not found.", request.Id);
            return StockErrors.StockItemNotFound;
        }

        return stockItem.ToStockItemDto();
    }
}
