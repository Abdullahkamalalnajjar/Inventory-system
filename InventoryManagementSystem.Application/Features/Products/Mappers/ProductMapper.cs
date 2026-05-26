using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Domain.Product;

namespace InventoryManagementSystem.Application.Features.Products.Mappers;

public static class ProductMapper
{
    public static ProductDto ToProductDto(this Product product, int totalQuantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.CategoryId,
            product.Category?.Name,
            product.UnitId,
            product.Price,
            totalQuantity);
    }

    public static List<ProductDto> ToProductDtoList(this IEnumerable<Product> products, IReadOnlyDictionary<Guid, int> totalQuantities)
    {
        return products
            .Select(product => product.ToProductDto(totalQuantities.GetValueOrDefault(product.Id)))
            .ToList();
    }
}
