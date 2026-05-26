using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<DeleteProductCommandHandler> logger) : IRequestHandler<DeleteProductCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<DeleteProductCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product with Id: {ProductId} Not found", request.ProductId);
            return ProductErrors.ProductNotFound;
        }

        bool hasReferences =
            await _context.SalesInvoiceItems.AnyAsync(i => i.ProductId == request.ProductId, cancellationToken) ||
            await _context.PurchaseInvoiceItems.AnyAsync(i => i.ProductId == request.ProductId, cancellationToken) ||
            await _context.StockItems.AnyAsync(s => s.ProductId == request.ProductId, cancellationToken) ||
            await _context.StockMovements.AnyAsync(m => m.ProductId == request.ProductId, cancellationToken);

        if (hasReferences)
        {
            _logger.LogWarning("Product with Id: {ProductId} cannot be deleted because it has related records", request.ProductId);
            return ProductErrors.CannotDeleteProductWithReferences;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(ProductsCacheKeys.ProductsList, cancellationToken);
        await _cache.RemoveByTagAsync(ProductsCacheKeys.ProductTag, cancellationToken);
        _logger.LogInformation("Product with Id: {ProductId} has been deleted successfully", product.Id);
        return Result.Deleted;
    }
}
