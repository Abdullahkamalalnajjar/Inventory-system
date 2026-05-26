using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Categories;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<UpdateCategoryCommandHandler> logger)
    : IRequestHandler<UpdateCategoryCommand, Result<Updated>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Category with id {CategoryId} not found.", request.CategoryId);
            return CategoryErrors.CategoryNotFound;
        }

        var normalizedName = request.Name.Trim().ToLower();
        var duplicateNameExists = await _context.Categories.AnyAsync(
            x => x.Id != request.CategoryId && x.Name.ToLower() == normalizedName,
            cancellationToken);

        if (duplicateNameExists)
        {
            _logger.LogWarning("Category update aborted. Name: {Name} already exists.", request.Name);
            return CategoryErrors.CategoryConflict;
        }

        var updateResult = category.Update(request.Name, request.Description);
        if (updateResult.IsError)
        {
            _logger.LogWarning("Error while updating category with id {CategoryId}.", request.CategoryId);
            return updateResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CategoriesCacheKeys.CategoriesListKey, cancellationToken);
        await _cache.RemoveByTagAsync(CategoriesCacheKeys.CategoriesTag, cancellationToken);

        _logger.LogInformation("Category updated successfully. Id: {CategoryId}", category.Id);
        return Result.Updated;
    }
}
