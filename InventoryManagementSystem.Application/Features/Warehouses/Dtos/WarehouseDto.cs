namespace InventoryManagementSystem.Application.Features.Warehouses.Dtos;

public class WarehouseDto(Guid id, string name, string? address)
{
    public Guid Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string? Address { get; set; } = address;
}
