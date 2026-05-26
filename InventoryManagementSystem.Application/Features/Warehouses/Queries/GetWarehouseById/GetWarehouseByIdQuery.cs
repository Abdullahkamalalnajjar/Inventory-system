using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Warehouses.Queries.GetWarehouseById;

public class GetWarehouseByIdQuery(Guid id) : IRequest<Result<WarehouseDto>>
{
    public Guid Id { get; set; } = id;
}
