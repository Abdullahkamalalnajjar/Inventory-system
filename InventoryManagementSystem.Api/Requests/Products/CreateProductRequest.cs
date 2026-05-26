namespace InventoryManagementSystem.Api.Requests.Products;

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId
);
