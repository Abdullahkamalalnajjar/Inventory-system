using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.UpdateStockItem;

public sealed record UpdateStockItemCommand(
    Guid StockItemId,
    int NewQuantity,
    string? ReferenceNumber = null,
    string? Notes = null
) : IRequest<Result<Updated>>;
