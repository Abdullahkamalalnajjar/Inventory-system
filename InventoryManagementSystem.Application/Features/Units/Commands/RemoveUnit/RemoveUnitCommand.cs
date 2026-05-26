using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Units.Commands.RemoveUnit;

public sealed record RemoveUnitCommand(Guid UnitId) : IRequest<Result<Deleted>>;
