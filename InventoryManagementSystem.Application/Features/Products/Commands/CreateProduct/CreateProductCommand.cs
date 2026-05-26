using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId
) : IRequest<Result<ProductDto>>;
