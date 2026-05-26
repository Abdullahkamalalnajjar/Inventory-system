using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Units.Commands.UpdateUnit;

public sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(x => x.UnitId)
            .NotEmpty().WithMessage("UnitId cannot be empty");

        RuleFor(x => x.UnitName)
            .NotEmpty().WithMessage("Unit name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required.")
            .MaximumLength(20);
    }
}
