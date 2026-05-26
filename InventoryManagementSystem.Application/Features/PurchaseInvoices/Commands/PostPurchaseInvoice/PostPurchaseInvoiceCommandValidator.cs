using FluentValidation;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.PostPurchaseInvoice;

public class PostPurchaseInvoiceCommandValidator : AbstractValidator<PostPurchaseInvoiceCommand>
{
    public PostPurchaseInvoiceCommandValidator()
    {
        RuleFor(x => x.PurchaseInvoiceId).NotEmpty();
    }
}
