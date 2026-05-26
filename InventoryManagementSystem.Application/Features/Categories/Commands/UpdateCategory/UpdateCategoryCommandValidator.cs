using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator :AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(c => c.CategoryId).NotEmpty().WithMessage("CategoryId cannot be empty");
        RuleFor(c => c.Name).NotEmpty().WithMessage("Name cannot be empty");
    }
}