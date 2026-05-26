using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.RemoveSalesInvoiceItem;

public sealed record RemoveSalesInvoiceItemCommand(
    Guid SalesInvoiceId,
    Guid SalesInvoiceItemId
) : IRequest<Result<Deleted>>;
