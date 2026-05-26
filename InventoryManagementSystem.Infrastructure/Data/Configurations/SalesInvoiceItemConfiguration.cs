using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class SalesInvoiceItemConfiguration : IEntityTypeConfiguration<SalesInvoiceItem>
{
    public void Configure(EntityTypeBuilder<SalesInvoiceItem> builder)
    {
        builder.ToTable("SalesInvoiceItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2);

        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
