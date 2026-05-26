namespace InventoryManagementSystem.Api.Requests.Units;

public sealed record CreateUnitRequest(
    string? UnitName,
    string? Symbol
);
