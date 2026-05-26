using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.CreateWarehouse;

public sealed record CreateWarehouseCommand
    (string Name, string? Address) : IRequest<Result<WarehouseDto>>;
