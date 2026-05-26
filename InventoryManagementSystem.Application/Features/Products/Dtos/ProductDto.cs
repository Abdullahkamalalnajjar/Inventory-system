namespace InventoryManagementSystem.Application.Features.Products.Dtos;

public record ProductDto(
    Guid ProductId,
    string? Name,
    string? Description,
    Guid CategoryId,
    string? CategoryName,
    Guid UnitId,
    decimal Price,
    int Quantity);
