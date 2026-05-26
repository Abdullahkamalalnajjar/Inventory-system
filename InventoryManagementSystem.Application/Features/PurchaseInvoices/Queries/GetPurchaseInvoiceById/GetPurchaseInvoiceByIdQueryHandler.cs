using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById;

public sealed class GetPurchaseInvoiceByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetPurchaseInvoiceByIdQueryHandler> logger)
    : IRequestHandler<GetPurchaseInvoiceByIdQuery, Result<PurchaseInvoiceDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetPurchaseInvoiceByIdQueryHandler> _logger = logger;

    public async Task<Result<PurchaseInvoiceDto>> Handle(GetPurchaseInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.PurchaseInvoices
            .AsNoTracking()
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Purchase invoice with id {InvoiceId} not found.", request.Id);
            return InvoiceErrors.InvoiceNotFound;
        }

        return invoice.ToPurchaseInvoiceDto();
    }
}
