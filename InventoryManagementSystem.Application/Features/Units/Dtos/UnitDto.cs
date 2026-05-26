namespace InventoryManagementSystem.Application.Features.Units.Dtos;

public class UnitDto(Guid id,string unitName, string symbol)
{
    public Guid Id { get; set; } = id;
    public string UnitName { get;  } = unitName;
    public string Symbol { get;  } = symbol;
}