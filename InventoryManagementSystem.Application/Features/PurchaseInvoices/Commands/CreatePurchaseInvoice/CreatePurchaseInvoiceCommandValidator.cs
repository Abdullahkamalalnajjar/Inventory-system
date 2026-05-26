using FluentValidation;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice;

public class CreatePurchaseInvoiceCommandValidator : AbstractValidator<CreatePurchaseInvoiceCommand>
{
    public CreatePurchaseInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WarehouseId).NotEmpty();
    }
}
