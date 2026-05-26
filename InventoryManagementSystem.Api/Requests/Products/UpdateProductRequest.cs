namespace InventoryManagementSystem.Api.Requests.Products;
public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    decimal Price
);
