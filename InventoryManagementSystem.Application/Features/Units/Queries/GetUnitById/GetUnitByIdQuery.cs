using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Units.Queries.GetUnitById;

public class GetUnitByIdQuery(Guid id) : IRequest<Result<UnitDto>>
{
    public Guid Id { get; set; } = id;
}
