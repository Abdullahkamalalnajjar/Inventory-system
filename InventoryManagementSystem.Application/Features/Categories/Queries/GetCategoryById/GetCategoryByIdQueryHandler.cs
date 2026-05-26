using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Application.Features.Categories.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler (IApplicationDbContext context , ILogger<GetCategoryByIdQueryHandler> logger)
:IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger = logger;

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync([request.Id],cancellationToken);
        if (category is null)
        {
            _logger.LogError($"Category with id {request.Id} not found");
            return CategoryErrors.CategoryNotFound;
        }
        return category.ToCategoryDto();
    }
}