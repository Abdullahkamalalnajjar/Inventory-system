using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Products.Queries.GetProducts;

public class GetProductsQuery : ICachedQuery<Result<List<ProductDto>>>
{
    public string CacheKey => ProductsCacheKeys.ProductsList;
    public TimeSpan? Expiration  => TimeSpan.FromMinutes(5);
    public IEnumerable<string>? Tags => [ProductsCacheKeys.ProductTag];
}