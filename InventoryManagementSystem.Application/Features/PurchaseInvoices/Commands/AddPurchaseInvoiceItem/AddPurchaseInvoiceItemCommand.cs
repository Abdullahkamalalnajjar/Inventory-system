using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.AddPurchaseInvoiceItem;

public sealed record AddPurchaseInvoiceItemCommand(
    Guid PurchaseInvoiceId,
    Guid ProductId,
    int Quantity,
    decimal UnitCost
) : IRequest<Result<PurchaseInvoiceItemDto>>;
