using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Application.Features.Categories;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery() : ICachedQuery<Result<List<CategoryDto>>>
{
    public string CacheKey => CategoriesCacheKeys.CategoriesListKey;

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

    public IEnumerable<string>? Tags => [CategoriesCacheKeys.CategoriesTag];
}
