using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Product;

public sealed class Unit : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Symbol { get; private set; } = string.Empty;

    private Unit()
    {
    }

    private Unit(string name, string symbol)
    {
        Name = name;
        Symbol = symbol;
    }

    public static Result<Unit> Create(string name, string symbol)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductErrors.UnitNameRequired;

        if (string.IsNullOrWhiteSpace(symbol))
            return ProductErrors.UnitSymbolRequired;

        return new Unit(name.Trim(), symbol.Trim());
    }

    public Result<Updated> Update(string name, string symbol)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductErrors.UnitNameRequired;

        if (string.IsNullOrWhiteSpace(symbol))
            return ProductErrors.UnitSymbolRequired;

        Name = name.Trim();
        Symbol = symbol.Trim();

        return Result.Updated;
    }
}
