using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.DeleteSalesInvoice;

public sealed class DeleteSalesInvoiceCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<DeleteSalesInvoiceCommandHandler> logger)
    : IRequestHandler<DeleteSalesInvoiceCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<DeleteSalesInvoiceCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(DeleteSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.SalesInvoices.FindAsync([request.SalesInvoiceId], cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Sales invoice with id {InvoiceId} not found.", request.SalesInvoiceId);
            return InvoiceErrors.InvoiceNotFound;
        }

        _context.SalesInvoices.Remove(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(SalesInvoicesCacheKeys.SalesInvoicesListKey, cancellationToken);
        _logger.LogInformation("Sales invoice deleted. Id: {InvoiceId}", request.SalesInvoiceId);

        return Result.Deleted;
    }
}
