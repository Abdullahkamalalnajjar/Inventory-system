using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.DeletePurchaseInvoice;

public sealed record DeletePurchaseInvoiceCommand(Guid PurchaseInvoiceId) : IRequest<Result<Deleted>>;
