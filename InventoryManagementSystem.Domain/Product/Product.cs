using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Product;

public sealed class Product : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public Guid UnitId { get; private set; }

    public decimal Price { get; private set; }
  
    
    private Product() {}
    private Product(string name, string? description, Guid categoryId, Guid unitId)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        UnitId = unitId;
        Price = 0;
    }

    public static Result<Product> Create(string name, Guid categoryId, Guid unitId, string? description = null)
    {
        if (string.IsNullOrEmpty(name)) 
          return  ProductErrors.NameRequired;
        if (categoryId == Guid.Empty)
            return ProductErrors.CategoryRequired;
        if (unitId == Guid.Empty)
            return ProductErrors.UnitRequired;

        return new Product (name, description, categoryId, unitId);
    }

    public Result<Updated> Update(string name, Guid categoryId, Guid unitId, decimal price, string? description = null)
    {
        if (string.IsNullOrEmpty(name)) 
            return  ProductErrors.NameRequired;
        if (categoryId == Guid.Empty)
            return ProductErrors.CategoryRequired;
        if (unitId == Guid.Empty)
            return ProductErrors.UnitRequired;
        if (price < 0)
            return  ProductErrors.PriceInvalid;

        Name = name;
        Description = description;
        CategoryId = categoryId;
        UnitId = unitId;
        Price = price;

        return Result.Updated;
    }
    
}
