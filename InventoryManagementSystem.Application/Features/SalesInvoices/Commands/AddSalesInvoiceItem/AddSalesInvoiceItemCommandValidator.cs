using FluentValidation;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.AddSalesInvoiceItem;

public class AddSalesInvoiceItemCommandValidator : AbstractValidator<AddSalesInvoiceItemCommand>
{
    public AddSalesInvoiceItemCommandValidator()
    {
        RuleFor(x => x.SalesInvoiceId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
