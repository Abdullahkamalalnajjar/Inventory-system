using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.CreateStockItem;

public class CreateStockItemCommandValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required");
        RuleFor(x => x.WarehouseId).NotEmpty().WithMessage("Warehouse is required");
        RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0).WithMessage("Initial quantity must be zero or greater");
    }
}
