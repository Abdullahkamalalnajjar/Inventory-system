using InventoryManagementSystem.Domain.Stock;

namespace InventoryManagementSystem.Application.Features.Stock.Dtos;

public record StockMovementDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    StockMovementType Type,
    int Quantity,
    string? ReferenceNumber,
    string? Notes,
    DateTimeOffset CreatedAtUtc);
