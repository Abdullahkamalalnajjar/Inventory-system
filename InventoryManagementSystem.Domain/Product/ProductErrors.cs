using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Product;

public static class ProductErrors
{
    public static Error ProductNotFound =>
        Error.NotFound("Product_Not_Found", "Product not found");

    public static Error NameRequired =>
        Error.Validation("Name_Required", "Product name is required");
    public static Error PriceInvalid =>
        Error.Validation("Price_Invalid", "Product price cannot be negative.");
    public static Error QuantityRequired =>
        Error.Validation("Quantity_Required", "Quantity is required");
    public static Error QuantityInvalid => 
        Error.Validation("Quantity_Invalid", "Product quantity cannot be negative.");
    
    public static Error CategoryRequired =>
        Error.Validation("Category_Required", "Category is required");

    public static Error UnitRequired =>
        Error.Validation("Unit_Required", "Unit is required.");

    public static Error UnitNameRequired =>
        Error.Validation("Unit_Name_Required", "Unit name is required.");

    public static Error UnitSymbolRequired =>
        Error.Validation("Unit_Symbol_Required", "Unit symbol is required.");

    public static readonly Error DuplicateName = Error.Conflict(
        "Product.DuplicateName",
        "Product name already exists.");
    
}
