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

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(stockItem => stockItem.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(stockItem => stockItem.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(stockItem => stockItem.Movements)
            .WithOne()
            .HasForeignKey("StockItemId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(stockItem => stockItem.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
