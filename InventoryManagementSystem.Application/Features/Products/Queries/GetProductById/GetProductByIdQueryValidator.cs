using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithErrorCode("ProductId_IsRequired")
            .WithMessage("Id cannot be empty");
    }
}