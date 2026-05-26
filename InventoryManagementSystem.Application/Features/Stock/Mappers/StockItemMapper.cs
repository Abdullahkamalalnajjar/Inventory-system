using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Domain.Stock;

namespace InventoryManagementSystem.Application.Features.Stock.Mappers;

public static class StockItemMapper
{
    public static StockItemDto ToStockItemDto(this StockItem stockItem)
    {
        ArgumentNullException.ThrowIfNull(stockItem);
        return new StockItemDto(
            stockItem.Id,
            stockItem.ProductId,
            stockItem.Product?.Name ?? string.Empty,
            stockItem.WarehouseId,
            stockItem.Warehouse?.Name ?? string.Empty,
            stockItem.QuantityOnHand);
    }

    public static List<StockItemDto> ToStockItemDtoList(this IEnumerable<StockItem> stockItems)
    {
        return stockItems.Select(x => x.ToStockItemDto()).ToList();
    }
}
