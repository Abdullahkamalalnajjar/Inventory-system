using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Application.Features.SalesInvoices.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Queries.GetSalesInvoices;

public sealed class GetSalesInvoicesQueryHandler(
    IApplicationDbContext context,
    ILogger<GetSalesInvoicesQueryHandler> logger)
    : IRequestHandler<GetSalesInvoicesQuery, Result<List<SalesInvoiceDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetSalesInvoicesQueryHandler> _logger = logger;

    public async Task<Result<List<SalesInvoiceDto>>> Handle(GetSalesInvoicesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching sales invoices list.");
        var invoices = await _context.SalesInvoices
            .AsNoTracking()
            .Include(i => i.Warehouse)
            .Include(i => i.Items)
            .ThenInclude(item => item.Product)
            .OrderByDescending(i => i.InvoiceDateUtc)
            .ToListAsync(cancellationToken);

        return invoices.ToSalesInvoiceDtoList();
    }
}
