using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.PostSalesInvoice;

public sealed record PostSalesInvoiceCommand(Guid SalesInvoiceId) : IRequest<Result<Updated>>;
