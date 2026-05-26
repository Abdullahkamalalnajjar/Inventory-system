using FluentValidation;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.RemovePurchaseInvoiceItem;

public class RemovePurchaseInvoiceItemCommandValidator : AbstractValidator<RemovePurchaseInvoiceItemCommand>
{
    public RemovePurchaseInvoiceItemCommandValidator()
    {
        RuleFor(x => x.PurchaseInvoiceId).NotEmpty();
        RuleFor(x => x.PurchaseInvoiceItemId).NotEmpty();
    }
}
