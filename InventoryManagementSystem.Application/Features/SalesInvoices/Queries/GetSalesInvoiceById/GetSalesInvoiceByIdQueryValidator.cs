using FluentValidation;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Queries.GetSalesInvoiceById;

public class GetSalesInvoiceByIdQueryValidator : AbstractValidator<GetSalesInvoiceByIdQuery>
{
    public GetSalesInvoiceByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
