using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Units.Commands.UpdateUnit;

public sealed record UpdateUnitCommand(
    Guid UnitId,
    string UnitName,
    string Symbol) : IRequest<Result<Updated>>;
