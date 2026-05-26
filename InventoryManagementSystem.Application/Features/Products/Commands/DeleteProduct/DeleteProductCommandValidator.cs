using FluentValidation;

namespace InventoryManagementSystem.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty()
            .WithErrorCode("ProductId_IsRequired")
            .WithMessage("ProductId is required");
    }
}
