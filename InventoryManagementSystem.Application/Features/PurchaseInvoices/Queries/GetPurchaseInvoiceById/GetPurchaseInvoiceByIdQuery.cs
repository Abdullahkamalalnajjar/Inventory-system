using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById;

public sealed record GetPurchaseInvoiceByIdQuery(Guid Id) : IRequest<Result<PurchaseInvoiceDto>>;
