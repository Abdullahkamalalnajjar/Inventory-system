using FluentValidation;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.PostSalesInvoice;

public class PostSalesInvoiceCommandValidator : AbstractValidator<PostSalesInvoiceCommand>
{
    public PostSalesInvoiceCommandValidator()
    {
        RuleFor(x => x.SalesInvoiceId).NotEmpty();
    }
}
