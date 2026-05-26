using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.AddSalesInvoiceItem;

public sealed record AddSalesInvoiceItemCommand(
    Guid SalesInvoiceId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
) : IRequest<Result<SalesInvoiceItemDto>>;
