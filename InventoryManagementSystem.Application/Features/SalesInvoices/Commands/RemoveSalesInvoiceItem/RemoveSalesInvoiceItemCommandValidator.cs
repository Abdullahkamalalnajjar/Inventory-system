using FluentValidation;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.RemoveSalesInvoiceItem;

public class RemoveSalesInvoiceItemCommandValidator : AbstractValidator<RemoveSalesInvoiceItemCommand>
{
    public RemoveSalesInvoiceItemCommandValidator()
    {
        RuleFor(x => x.SalesInvoiceId).NotEmpty();
        RuleFor(x => x.SalesInvoiceItemId).NotEmpty();
    }
}
