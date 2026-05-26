using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Categories;
using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Application.Features.Categories.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreateCategoryCommandHandler> logger)
    :IRequestHandler<CreateCategoryCommand,Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreateCategoryCommandHandler> _logger = logger;

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim().ToLower();
        var exists = await _context.Categories.AnyAsync(x => x.Name.ToLower() == name,cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Category creation aborted. Name: {name} already exists.", name);
            return CategoryErrors.CategoryConflict;
            
        }
        var createCategoryResult = Category.Create(request.Name, request.Description);
        if (createCategoryResult.IsError)
        {
            _logger.LogWarning("Exist Error When Make Create Category: {name}", request.Name);
            return createCategoryResult.Errors;
        }

        await _context.Categories.AddAsync(createCategoryResult.Value,cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CategoriesCacheKeys.CategoriesListKey, cancellationToken);
        await _cache.RemoveByTagAsync(CategoriesCacheKeys.CategoriesTag, cancellationToken);
        _logger.LogWarning("Category created successfully. Id: {CategoryId}",createCategoryResult.Value.Id);
        return createCategoryResult.Value.ToCategoryDto();
    }
}
