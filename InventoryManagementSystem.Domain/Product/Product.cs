using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Product;

public sealed class Product : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid UnitId { get; private set; }

    public decimal Price { get; private set; }

    public int Quantity { get; private set; }

    
    private Product() {}
    private Product(string name, string? description, Guid unitId, decimal price, int quantity)
    {
        Name = name;
        Description = description;
        UnitId = unitId;
        Price = price;
        Quantity = quantity;
    }

    public static Result<Product> Create(string name, Guid unitId, decimal price, int quantity, string? description = null)
    {
        if (string.IsNullOrEmpty(name)) 
          return  ProductErrors.NameRequired;
        if (unitId == Guid.Empty)
            return ProductErrors.UnitRequired;
        if (price < 0)
            return  ProductErrors.PriceInvalid;
        if (quantity < 0)
            return  ProductErrors.QuantityInvalid;
        return new Product (name, description, unitId, price, quantity);
    }

    public Result<Updated> Update(string name, Guid unitId, decimal price, int quantity, string? description = null)
    {
        if (string.IsNullOrEmpty(name)) 
            return  ProductErrors.NameRequired;
        if (unitId == Guid.Empty)
            return ProductErrors.UnitRequired;
        if (price < 0)
            return  ProductErrors.PriceInvalid;
        if (quantity < 0)
            return  ProductErrors.QuantityInvalid;

        Name = name;
        Description = description;
        UnitId = unitId;
        Price = price;
        Quantity = quantity;

        return Result.Updated;
    }
    
}
