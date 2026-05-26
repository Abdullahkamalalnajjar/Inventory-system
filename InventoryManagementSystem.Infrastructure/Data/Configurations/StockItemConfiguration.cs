using InventoryManagementSystem.Domain.Stock;
using InventoryManagementSystem.Domain.Product;
using InventoryManagementSystem.Domain.Warehouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");

        builder.HasKey(stockItem => stockItem.Id);

        builder.HasIndex(stockItem => new { stockItem.ProductId, stockItem.WarehouseId })
            .IsUnique();

        builder.HasOne(stockItem => stockItem.Product)
            .WithMany()
            .HasForeignKey(stockItem => stockItem.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(stockItem => stockItem.Warehouse)
            .WithMany()
            .HasForeignKey(stockItem => stockItem.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(stockItem => stockItem.Movements)
            .WithOne()
            .HasForeignKey(stockMovement => stockMovement.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(stockItem => stockItem.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
