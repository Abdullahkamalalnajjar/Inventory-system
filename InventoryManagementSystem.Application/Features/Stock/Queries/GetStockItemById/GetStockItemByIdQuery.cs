using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Stock.Queries.GetStockItemById;

public sealed record GetStockItemByIdQuery(Guid Id) : IRequest<Result<StockItemDto>>;
