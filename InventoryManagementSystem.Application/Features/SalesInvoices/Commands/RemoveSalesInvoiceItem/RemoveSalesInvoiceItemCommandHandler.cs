using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.RemoveSalesInvoiceItem;

public sealed class RemoveSalesInvoiceItemCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<RemoveSalesInvoiceItemCommandHandler> logger)
    : IRequestHandler<RemoveSalesInvoiceItemCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<RemoveSalesInvoiceItemCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(RemoveSalesInvoiceItemCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.SalesInvoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.SalesInvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Sales invoice with id {InvoiceId} not found.", request.SalesInvoiceId);
            return InvoiceErrors.InvoiceNotFound;
        }

        var removeResult = invoice.RemoveItem(request.SalesInvoiceItemId);
        if (removeResult.IsError)
        {
            return removeResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(SalesInvoicesCacheKeys.SalesInvoicesListKey, cancellationToken);
        _logger.LogInformation(
            "Item {ItemId} removed from sales invoice {InvoiceId}.",
            request.SalesInvoiceItemId,
            invoice.Id);

        return Result.Deleted;
    }
}
