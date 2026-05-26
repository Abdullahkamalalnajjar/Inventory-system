using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200);

        RuleFor(command => command.CategoryId)
            .NotEmpty()
            .WithMessage("CategoryId is required");

        RuleFor(command => command.UnitId)
            .NotEmpty()
            .WithMessage("UnitId is required");

        RuleFor(command => command.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0");
    }
}
