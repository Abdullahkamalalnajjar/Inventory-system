namespace InventoryManagementSystem.Api.Requests.Units;

public sealed record UpdateUnitRequest(
    string? UnitName,
    string? Symbol
);
