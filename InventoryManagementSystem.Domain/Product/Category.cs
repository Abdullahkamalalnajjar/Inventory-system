using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Product;

public sealed class Category : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    private Category()
    {
    }

    private Category(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public static Result<Category> Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductErrors.CategoryRequired;

        return new Category(name.Trim(), description);
    }

    public Result<Updated> Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductErrors.CategoryRequired;

        Name = name.Trim();
        Description = description;

        return Result.Updated;
    }
}
