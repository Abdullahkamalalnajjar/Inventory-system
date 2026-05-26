using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Units.Commands.RemoveUnit;

public sealed class RemoveUnitCommandValidator : AbstractValidator<RemoveUnitCommand>
{
    public RemoveUnitCommandValidator()
    {
        RuleFor(c => c.UnitId)
            .NotEmpty().WithErrorCode("UnitId_IsRequired").WithMessage("Id cannot be empty");
    }
}
