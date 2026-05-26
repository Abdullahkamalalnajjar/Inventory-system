using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQuery(Guid id) : IRequest<Result<ProductDto>>
{
    public Guid Id { get; } = id;
}