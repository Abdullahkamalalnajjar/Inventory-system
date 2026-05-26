using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand
(string Name , string? Description):IRequest<Result<CategoryDto>>;