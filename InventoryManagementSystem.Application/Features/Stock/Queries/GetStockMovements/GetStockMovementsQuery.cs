using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Stock.Queries.GetStockMovements;

public sealed record GetStockMovementsQuery(
    Guid? StockItemId = null,
    Guid? ProductId = null,
    Guid? WarehouseId = null
) : IRequest<Result<List<StockMovementDto>>>;
