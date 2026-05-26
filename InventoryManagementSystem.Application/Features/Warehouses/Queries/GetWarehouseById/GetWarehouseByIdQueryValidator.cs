using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Warehouses.Queries.GetWarehouseById;

public class GetWarehouseByIdQueryValidator : AbstractValidator<GetWarehouseByIdQuery>
{
    public GetWarehouseByIdQueryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithErrorCode("WarehouseId_Is_Required")
            .WithMessage("Warehouse id cannot be empty");
    }
}
