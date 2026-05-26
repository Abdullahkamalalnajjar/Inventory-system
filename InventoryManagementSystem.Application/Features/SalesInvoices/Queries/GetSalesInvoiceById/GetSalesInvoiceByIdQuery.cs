using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Queries.GetSalesInvoiceById;

public sealed record GetSalesInvoiceByIdQuery(Guid Id) : IRequest<Result<SalesInvoiceDto>>;
