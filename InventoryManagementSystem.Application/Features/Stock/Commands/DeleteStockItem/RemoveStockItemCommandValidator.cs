using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Stock.Commands.DeleteStockItem;

public class RemoveStockItemCommandValidator : AbstractValidator<RemoveStockItemCommand>
{
    public RemoveStockItemCommandValidator()
    {
        RuleFor(x => x.StockItemId).NotEmpty().WithMessage("Stock item id is required");
    }
}
