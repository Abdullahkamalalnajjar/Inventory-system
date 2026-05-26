using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.DeleteCategory;

public class RemoveCategoryCommandValidator : AbstractValidator<RemoveCategoryCommand>
{
    public RemoveCategoryCommandValidator()
    {
        RuleFor(command => command.CategoryId).NotEmpty().WithMessage("CategoryId is required");
    }
}