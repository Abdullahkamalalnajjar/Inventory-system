using FluentValidation;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.DeleteSalesInvoice;

public class DeleteSalesInvoiceCommandValidator : AbstractValidator<DeleteSalesInvoiceCommand>
{
    public DeleteSalesInvoiceCommandValidator()
    {
        RuleFor(x => x.SalesInvoiceId).NotEmpty();
    }
}
