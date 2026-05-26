using FluentValidation;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.AddPurchaseInvoiceItem;

public class AddPurchaseInvoiceItemCommandValidator : AbstractValidator<AddPurchaseInvoiceItemCommand>
{
    public AddPurchaseInvoiceItemCommandValidator()
    {
        RuleFor(x => x.PurchaseInvoiceId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}
