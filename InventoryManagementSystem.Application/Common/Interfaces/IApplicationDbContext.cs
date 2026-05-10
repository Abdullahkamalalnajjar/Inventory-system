using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Domain.Identity;
using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Product;
using InventoryManagementSystem.Domain.Stock;
using InventoryManagementSystem.Domain.Warehouse;

namespace InventoryManagementSystem.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Product> Products { get; }

    DbSet<Category> Categories { get; }

    DbSet<Unit> Units { get; }

    DbSet<Warehouse> Warehouses { get; }

    DbSet<StockItem> StockItems { get; }

    DbSet<StockMovement> StockMovements { get; }

    DbSet<PurchaseInvoice> PurchaseInvoices { get; }

    DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }

    DbSet<SalesInvoice> SalesInvoices { get; }

    DbSet<SalesInvoiceItem> SalesInvoiceItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
