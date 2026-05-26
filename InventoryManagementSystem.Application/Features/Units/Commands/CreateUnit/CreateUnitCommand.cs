using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Units.Commands.CreateUnitCommand;

public sealed record CreateUnitCommand
    (string UnitName, string Symbol):IRequest<Result<UnitDto>>;
