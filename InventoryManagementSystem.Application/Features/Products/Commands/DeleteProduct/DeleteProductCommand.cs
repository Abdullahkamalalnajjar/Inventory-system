using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : IRequest<Result<Deleted>>;
