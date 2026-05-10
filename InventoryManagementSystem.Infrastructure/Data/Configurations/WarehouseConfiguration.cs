using InventoryManagementSystem.Domain.Warehouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(warehouse => warehouse.Id);

        builder.Property(warehouse => warehouse.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(warehouse => warehouse.Address)
            .HasMaxLength(500);

        builder.HasIndex(warehouse => warehouse.Name)
            .IsUnique();
    }
}
