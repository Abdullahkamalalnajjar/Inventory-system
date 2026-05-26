using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.UpdateStockItem;

public class UpdateStockItemCommandValidator : AbstractValidator<UpdateStockItemCommand>
{
    public UpdateStockItemCommandValidator()
    {
        RuleFor(x => x.StockItemId).NotEmpty().WithMessage("Stock item id is required");
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0).WithMessage("Quantity must be zero or greater");
    }
}
