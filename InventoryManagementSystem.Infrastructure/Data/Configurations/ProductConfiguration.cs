using InventoryManagementSystem.Domain.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(1000);

        builder.HasOne(product => product.Category)
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(product => product.Price)
            .HasPrecision(18, 2);

        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(product => product.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(product => product.Name);
    }
}
