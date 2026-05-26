using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.RemovePurchaseInvoiceItem;

public sealed record RemovePurchaseInvoiceItemCommand(
    Guid PurchaseInvoiceId,
    Guid PurchaseInvoiceItemId
) : IRequest<Result<Deleted>>;
