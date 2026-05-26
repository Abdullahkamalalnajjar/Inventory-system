using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice;

public sealed record CreatePurchaseInvoiceCommand(
    string InvoiceNumber,
    Guid WarehouseId,
    Guid? SupplierId,
    DateTimeOffset? InvoiceDateUtc
) : IRequest<Result<PurchaseInvoiceDto>>;
