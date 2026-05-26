namespace InventoryManagementSystem.Application.Common.Security;

public static class AuthorizationPolicies
{
    public const string AuthenticatedUser = "auth.user";
    public const string CurrentUserRead = "current-user.read";
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string PaymentsCheckout = "payments.checkout";
    public const string CategoriesRead = "categories.read";
    public const string CategoriesWrite = "categories.write";
    public const string UnitsRead = "units.read";
    public const string UnitsWrite = "units.write";
    public const string ProductsRead = "products.read";
    public const string ProductsWrite = "products.write";
    public const string WarehousesRead = "warehouses.read";
    public const string WarehousesWrite = "warehouses.write";
    public const string StockRead = "stock.read";
    public const string StockWrite = "stock.write";
    public const string PurchaseInvoicesRead = "purchase-invoices.read";
    public const string PurchaseInvoicesWrite = "purchase-invoices.write";
    public const string SalesInvoicesRead = "sales-invoices.read";
    public const string SalesInvoicesWrite = "sales-invoices.write";
}
