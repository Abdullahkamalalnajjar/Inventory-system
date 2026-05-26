using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.CreateSalesInvoice;

public sealed record CreateSalesInvoiceCommand(
    string InvoiceNumber,
    Guid WarehouseId,
    Guid? CustomerId,
    DateTimeOffset? InvoiceDateUtc
) : IRequest<Result<SalesInvoiceDto>>;
