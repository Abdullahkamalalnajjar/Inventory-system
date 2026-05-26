using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.DeletePurchaseInvoice;

public sealed class DeletePurchaseInvoiceCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<DeletePurchaseInvoiceCommandHandler> logger)
    : IRequestHandler<DeletePurchaseInvoiceCommand, Result<Deleted>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<DeletePurchaseInvoiceCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(DeletePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.PurchaseInvoices.FindAsync([request.PurchaseInvoiceId], cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Purchase invoice with id {InvoiceId} not found.", request.PurchaseInvoiceId);
            return InvoiceErrors.InvoiceNotFound;
        }
        

        _context.PurchaseInvoices.Remove(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(PurchaseInvoicesCacheKeys.PurchaseInvoicesListKey, cancellationToken);
        _logger.LogInformation("Purchase invoice deleted. Id: {InvoiceId}", request.PurchaseInvoiceId);

        return Result.Deleted;
    }
}
