using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.DeleteWarehouse;

public sealed record RemoveWarehouseCommand
    (Guid WarehouseId) : IRequest<Result<Deleted>>;
