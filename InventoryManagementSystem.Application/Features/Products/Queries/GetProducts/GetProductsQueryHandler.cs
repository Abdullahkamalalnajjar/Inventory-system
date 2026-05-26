using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Application.Features.Products.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler(
    IApplicationDbContext context, 
    ILogger<GetProductsQueryHandler> logger)
    : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetProductsQueryHandler> _logger = logger;

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .ToListAsync(cancellationToken);

        var productIds = products.Select(product => product.Id).ToList();

        var totalQuantities = await _context.StockItems
            .AsNoTracking()
            .Where(stockItem => productIds.Contains(stockItem.ProductId))
            .GroupBy(stockItem => stockItem.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(stockItem => stockItem.QuantityOnHand)
            })
            .ToDictionaryAsync(
                item => item.ProductId,
                item => item.Quantity,
                cancellationToken);

        return products.ToProductDtoList(totalQuantities);
    }
}
