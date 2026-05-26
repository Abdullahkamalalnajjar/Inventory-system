using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.DeleteCategory;

public sealed record RemoveCategoryCommand
(Guid CategoryId):IRequest<Result<Deleted>>;