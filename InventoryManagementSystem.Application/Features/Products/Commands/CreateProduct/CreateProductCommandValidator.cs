using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");

        RuleFor(x => x.UnitId)
            .NotEmpty().WithMessage("UnitId is required");
    }
}
