using InventoryManagementSystem.Domain.Stock;
using InventoryManagementSystem.Domain.Product;
using InventoryManagementSystem.Domain.Warehouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(stockMovement => stockMovement.Id);

        builder.Property(stockMovement => stockMovement.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(stockMovement => stockMovement.Notes)
            .HasMaxLength(1000);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(stockMovement => stockMovement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(stockMovement => stockMovement.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(stockMovement => new { stockMovement.ProductId, stockMovement.WarehouseId });
    }
}
