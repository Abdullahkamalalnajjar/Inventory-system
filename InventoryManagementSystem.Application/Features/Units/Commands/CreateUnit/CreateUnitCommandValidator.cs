using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Units.Commands.CreateUnit;

public class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand.CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(x => x.UnitName)
            .NotEmpty().WithErrorCode("UnitName_IsRequired").WithMessage("Unit name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Symbol)
            .NotEmpty().WithErrorCode("Symbol_Name_IsRequired").WithMessage("Symbol is required.")
            .MaximumLength(20);
    }
}