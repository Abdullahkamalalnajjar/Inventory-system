using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Categories;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.DeleteCategory;

public class RemoveCategoryCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<RemoveCategoryCommandHandler> logger) :
    IRequestHandler<RemoveCategoryCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<RemoveCategoryCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(RemoveCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);
        if (category is null)
        {
            _logger.LogWarning("Category with id {CategoryId} not found.", request.CategoryId);
            return CategoryErrors.CategoryNotFound;
        }

        var hasProducts = await _context.Products.AnyAsync(
            x => x.CategoryId == request.CategoryId,
            cancellationToken);

        if (hasProducts)
        {
            _logger.LogWarning(
                "Category with id {CategoryId} cannot be deleted because it has associated products.",
                request.CategoryId);
            return CategoryErrors.CannotDeleteCategoryWithProducts;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CategoriesCacheKeys.CategoriesListKey, cancellationToken);
        await _cache.RemoveByTagAsync(CategoriesCacheKeys.CategoriesTag, cancellationToken);

        return Result.Deleted;
    }
}
