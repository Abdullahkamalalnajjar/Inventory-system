using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Invoices;

public static class InvoiceErrors
{
    public static Error InvoiceNumberRequired =>
        Error.Validation("Invoice_Number_Required", "Invoice number is required.");

    public static Error WarehouseRequired =>
        Error.Validation("Invoice_Warehouse_Required", "Warehouse is required.");

    public static Error ProductRequired =>
        Error.Validation("Invoice_Product_Required", "Product is required.");

    public static Error QuantityInvalid =>
        Error.Validation("Invoice_Quantity_Invalid", "Quantity must be greater than zero.");

    public static Error UnitPriceInvalid =>
        Error.Validation("Invoice_Unit_Price_Invalid", "Unit price cannot be negative.");

    public static Error EmptyInvoice =>
        Error.Validation("Invoice_Empty", "Invoice must contain at least one item.");

    public static Error AlreadyPosted =>
        Error.Conflict("Invoice_Already_Posted", "Invoice is already posted.");

    public static Error CannotChangePostedInvoice =>
        Error.Conflict("Invoice_Cannot_Change_Posted", "Posted invoice cannot be changed.");

    public static Error InvoiceNotFound =>
        Error.NotFound("Invoice_Not_Found", "Invoice not found.");

    public static Error InvoiceItemNotFound =>
        Error.NotFound("Invoice_Item_Not_Found", "Invoice item not found.");

    public static Error InvoiceNumberExists =>
        Error.Conflict("Invoice_Number_Exists", "Invoice number already exists.");
}
