using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(c => c.WarehouseId).NotEmpty().WithMessage("WarehouseId cannot be empty");
        RuleFor(c => c.Name).NotEmpty().WithMessage("Name cannot be empty");
    }
}
