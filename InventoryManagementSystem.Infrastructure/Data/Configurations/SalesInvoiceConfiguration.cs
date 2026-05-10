using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Warehouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.ToTable("SalesInvoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.InvoiceNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(invoice => invoice.InvoiceNumber)
            .IsUnique();

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(invoice => invoice.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey("SalesInvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
