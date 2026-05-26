using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<UpdateProductCommandHandler> logger) : IRequestHandler<UpdateProductCommand, Result<Updated>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<UpdateProductCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

        if (product is null)
        {
            _logger.LogWarning("Product with id {ProductId} not found.", request.ProductId);
            return ProductErrors.ProductNotFound;
        }

        var categoryExists = await _context.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            _logger.LogWarning("Product update aborted. Category with id {CategoryId} not found.", request.CategoryId);
            return ProductErrors.CategoryNotFound;
        }

        var unitExists = await _context.Units
            .AsNoTracking()
            .AnyAsync(unit => unit.Id == request.UnitId, cancellationToken);

        if (!unitExists)
        {
            _logger.LogWarning("Product update aborted. Unit with id {UnitId} not found.", request.UnitId);
            return ProductErrors.UnitNotFound;
        }

        var duplicateNameExists = await _context.Products
            .AsNoTracking()
            .AnyAsync(
                product => product.Id != request.ProductId && product.Name == request.Name,
                cancellationToken);

        if (duplicateNameExists)
        {
            _logger.LogWarning("Product update aborted. Name {Name} already exists.", request.Name);
            return ProductErrors.DuplicateName;
        }

        var updateResult = product.Update(
            request.Name,
            request.CategoryId,
            request.UnitId,
            request.Price,
            request.Description);

        if (updateResult.IsError)
        {
            _logger.LogWarning("Error while updating product with id {ProductId}.", request.ProductId);
            return updateResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);

        _logger.LogInformation("Product updated successfully. Id: {ProductId}", product.Id);
        return Result.Updated;
    }
}
