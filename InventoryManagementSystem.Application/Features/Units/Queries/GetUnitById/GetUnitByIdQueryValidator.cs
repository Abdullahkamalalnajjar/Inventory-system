using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Units.Queries.GetUnitById;

public class GetUnitByIdQueryValidator : AbstractValidator<GetUnitByIdQuery>
{
    public GetUnitByIdQueryValidator()
    {
        RuleFor(u => u.Id)
            .NotEmpty()
            .WithErrorCode("UnitId_Is_Required")
            .WithMessage("Unit id cannot be empty");
    }
}
