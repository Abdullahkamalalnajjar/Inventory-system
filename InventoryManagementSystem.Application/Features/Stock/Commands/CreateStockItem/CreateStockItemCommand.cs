using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.CreateStockItem;

public sealed record CreateStockItemCommand(
    Guid ProductId,
    Guid WarehouseId,
    int InitialQuantity
) : IRequest<Result<StockItemDto>>;
