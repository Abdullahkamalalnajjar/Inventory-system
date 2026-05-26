using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdQueryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithErrorCode("CategoryId_Is_Required")
            .WithMessage("Category id cannot be empty");
    }
}