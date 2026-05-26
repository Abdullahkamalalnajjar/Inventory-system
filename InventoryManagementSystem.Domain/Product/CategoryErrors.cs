using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Domain.Product;

public static class CategoryErrors
{
    public static Error CategoryNotFound =>
        Error.NotFound("Category_Not_Found", "Category not found");
    public static Error NameRequired =>
        Error.Validation("Name_Required", "Name is required");
    public static Error CategoryConflict =>
        Error.Conflict("Category_Conflict", "Category conflict");
    public static Error CannotDeleteCategoryWithProducts =>
        Error.Conflict("Category_CannotDelete", "Cannot delete category with products");
     

}