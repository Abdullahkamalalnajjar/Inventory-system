using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.UpdateWarehouse;

public sealed record UpdateWarehouseCommand
    (Guid WarehouseId, string Name, string? Address) : IRequest<Result<Updated>>;
