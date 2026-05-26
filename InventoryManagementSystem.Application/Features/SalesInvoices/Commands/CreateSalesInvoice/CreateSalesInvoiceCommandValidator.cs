using FluentValidation;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.CreateSalesInvoice;

public class CreateSalesInvoiceCommandValidator : AbstractValidator<CreateSalesInvoiceCommand>
{
    public CreateSalesInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WarehouseId).NotEmpty();
    }
}
