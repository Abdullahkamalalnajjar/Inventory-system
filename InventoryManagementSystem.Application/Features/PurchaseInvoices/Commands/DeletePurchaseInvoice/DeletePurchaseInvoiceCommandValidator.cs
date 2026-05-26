using FluentValidation;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.DeletePurchaseInvoice;

public class DeletePurchaseInvoiceCommandValidator : AbstractValidator<DeletePurchaseInvoiceCommand>
{
    public DeletePurchaseInvoiceCommandValidator()
    {
        RuleFor(x => x.PurchaseInvoiceId).NotEmpty();
    }
}
