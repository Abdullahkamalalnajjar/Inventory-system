using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.PostPurchaseInvoice;

public sealed record PostPurchaseInvoiceCommand(Guid PurchaseInvoiceId) : IRequest<Result<Updated>>;
