using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.RemovePurchaseInvoiceItem;

public sealed class RemovePurchaseInvoiceItemCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<RemovePurchaseInvoiceItemCommandHandler> logger)
    : IRequestHandler<RemovePurchaseInvoiceItemCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<RemovePurchaseInvoiceItemCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(RemovePurchaseInvoiceItemCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.PurchaseInvoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.PurchaseInvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Purchase invoice with id {InvoiceId} not found.", request.PurchaseInvoiceId);
            return InvoiceErrors.InvoiceNotFound;
        }

        var removeResult = invoice.RemoveItem(request.PurchaseInvoiceItemId);
        if (removeResult.IsError)
        {
            return removeResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(PurchaseInvoicesCacheKeys.PurchaseInvoicesListKey, cancellationToken);
        _logger.LogInformation(
            "Item {ItemId} removed from purchase invoice {InvoiceId}.",
            request.PurchaseInvoiceItemId,
            invoice.Id);

        return Result.Deleted;
    }
}
