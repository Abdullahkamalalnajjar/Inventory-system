using InventoryManagementSystem.Domain.Identity;

namespace InventoryManagementSystem.Application.Common.Security;

public static class Permissions
{
    public static class Users
    {
        public const string ReadSelf = "users:self:read";
        public const string Read = "users:read";
        public const string Write = "users:write";
    }

    public static class Payments
    {
        public const string Checkout = "payments:checkout";
    }

    public static class Categories
    {
        public const string Read = "categories:read";
        public const string Write = "categories:write";
    }

    public static class Units
    {
        public const string Read = "units:read";
        public const string Write = "units:write";
    }

    public static class Products
    {
        public const string Read = "products:read";
        public const string Write = "products:write";
    }

    public static class Warehouses
    {
        public const string Read = "warehouses:read";
        public const string Write = "warehouses:write";
    }

    public static class Stock
    {
        public const string Read = "stock:read";
        public const string Write = "stock:write";
    }

    public static class PurchaseInvoices
    {
        public const string Read = "purchase-invoices:read";
        public const string Write = "purchase-invoices:write";
    }

    public static class SalesInvoices
    {
        public const string Read = "sales-invoices:read";
        public const string Write = "sales-invoices:write";
    }

    public static IReadOnlyCollection<string> GetForRole(string role)
    {
        if (string.Equals(role, Role.Manager.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                Users.ReadSelf,
                Users.Read,
                Users.Write,
                Payments.Checkout,
                Categories.Read,
                Categories.Write,
                Units.Read,
                Units.Write,
                Products.Read,
                Products.Write,
                Warehouses.Read,
                Warehouses.Write,
                Stock.Read,
                Stock.Write,
                PurchaseInvoices.Read,
                PurchaseInvoices.Write,
                SalesInvoices.Read,
                SalesInvoices.Write
            ];
        }

        if (string.Equals(role, Role.Member.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                Users.ReadSelf,
                Payments.Checkout
            ];
        }

        return [];
    }
}
