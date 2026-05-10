using InventoryManagementSystem.Domain.Common;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Warehouse;

public sealed class Warehouse : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Address { get; private set; }

    private Warehouse()
    {
    }

    private Warehouse(string name, string? address)
    {
        Name = name;
        Address = address;
    }

    public static Result<Warehouse> Create(string name, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return WarehouseErrors.NameRequired;

        return new Warehouse(name.Trim(), address);
    }

    public Result<Updated> Update(string name, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return WarehouseErrors.NameRequired;

        Name = name.Trim();
        Address = address;

        return Result.Updated;
    }
}
