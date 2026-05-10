using InventoryManagementSystem.Domain.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");

        builder.HasKey(unit => unit.Id);

        builder.Property(unit => unit.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(unit => unit.Symbol)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(unit => unit.Name)
            .IsUnique();
    }
}
