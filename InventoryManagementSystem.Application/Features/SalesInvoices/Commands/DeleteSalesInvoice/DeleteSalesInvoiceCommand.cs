using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.DeleteSalesInvoice;

public sealed record DeleteSalesInvoiceCommand(Guid SalesInvoiceId) : IRequest<Result<Deleted>>;
