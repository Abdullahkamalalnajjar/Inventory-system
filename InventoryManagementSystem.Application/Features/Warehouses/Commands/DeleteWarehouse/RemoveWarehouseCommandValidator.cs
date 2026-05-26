using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Warehouses.Commands.DeleteWarehouse;

public class RemoveWarehouseCommandValidator : AbstractValidator<RemoveWarehouseCommand>
{
    public RemoveWarehouseCommandValidator()
    {
        RuleFor(command => command.WarehouseId).NotEmpty().WithMessage("WarehouseId is required");
    }
}
