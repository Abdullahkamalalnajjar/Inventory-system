using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.DeleteStockItem;

public sealed record RemoveStockItemCommand(Guid StockItemId) : IRequest<Result<Deleted>>;
