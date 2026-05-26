using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQuery(Guid id) : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; } = id;
}