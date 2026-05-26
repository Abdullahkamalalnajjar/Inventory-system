using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class PurchaseInvoiceItemConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> builder)
    {
        builder.ToTable("PurchaseInvoiceItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.UnitCost)
            .HasPrecision(18, 2);

        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
