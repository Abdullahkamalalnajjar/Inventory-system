using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoices;

public sealed class GetPurchaseInvoicesQueryHandler(
    IApplicationDbContext context,
    ILogger<GetPurchaseInvoicesQueryHandler> logger)
    : IRequestHandler<GetPurchaseInvoicesQuery, Result<List<PurchaseInvoiceDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetPurchaseInvoicesQueryHandler> _logger = logger;

    public async Task<Result<List<PurchaseInvoiceDto>>> Handle(GetPurchaseInvoicesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching purchase invoices list.");
        var invoices = await _context.PurchaseInvoices
            .AsNoTracking()
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
            .ThenInclude(item => item.Product)
            .OrderByDescending(i => i.InvoiceDateUtc)
            .ToListAsync(cancellationToken);

        return invoices.ToPurchaseInvoiceDtoList();
    }
}
