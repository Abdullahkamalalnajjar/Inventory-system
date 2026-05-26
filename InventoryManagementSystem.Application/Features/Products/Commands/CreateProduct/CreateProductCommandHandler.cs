using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products;
using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Application.Features.Products.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreateProductCommandHandler> _logger = logger;

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            _logger.LogError($"Category with id {request.CategoryId} does not exist");
            return ProductErrors.CategoryNotFound;
        }

        var unit = await _context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken);

        if (unit is null)
        {
            _logger.LogError($"Unit with id {request.UnitId} does not exist");
            return ProductErrors.UnitNotFound;
        }

        var nameExists = await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Product with name {name} already exists", request.Name);
            return ProductErrors.DuplicateName;
        }

        var createResult = Product.Create(
            request.Name,
            request.CategoryId,
            request.UnitId,
            request.Description);

        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        var product = createResult.Value;

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);
        _logger.LogInformation("Product created successfully. Id:{ProductId}", product.Id);
        return product.ToProductDto(0);
        
    }
}
