using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);
        RuleFor(x => x.Address)
            .MaximumLength(500);
    }
}
