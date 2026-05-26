using FluentValidation;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById;

public class GetPurchaseInvoiceByIdQueryValidator : AbstractValidator<GetPurchaseInvoiceByIdQuery>
{
    public GetPurchaseInvoiceByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
