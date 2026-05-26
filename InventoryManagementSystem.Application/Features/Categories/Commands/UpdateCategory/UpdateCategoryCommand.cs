using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand
(Guid CategoryId , string Name , string? Description):IRequest<Result<Updated>>;