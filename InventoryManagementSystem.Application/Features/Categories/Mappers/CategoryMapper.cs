using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Domain.Product;

namespace InventoryManagementSystem.Application.Features.Categories.Mappers;

public static class CategoryMapper
{
    public static CategoryDto ToCategoryDto(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public static List<CategoryDto> ToCategoryDtoList(this IEnumerable<Category> categories)
    {
        return categories.Select(x=>x.ToCategoryDto()).ToList();
    }
}