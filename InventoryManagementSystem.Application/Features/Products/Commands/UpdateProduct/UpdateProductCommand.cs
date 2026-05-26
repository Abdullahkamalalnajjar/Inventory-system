using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    decimal Price
) : IRequest<Result<Updated>>;
