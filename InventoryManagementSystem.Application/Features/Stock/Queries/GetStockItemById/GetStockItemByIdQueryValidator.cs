using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Stock.Queries.GetStockItemById;

public class GetStockItemByIdQueryValidator : AbstractValidator<GetStockItemByIdQuery>
{
    public GetStockItemByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Stock item id is required");
    }
}
