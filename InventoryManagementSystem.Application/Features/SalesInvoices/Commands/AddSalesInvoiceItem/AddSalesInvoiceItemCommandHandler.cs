using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Application.Features.SalesInvoices.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.AddSalesInvoiceItem;

public sealed class AddSalesInvoiceItemCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<AddSalesInvoiceItemCommandHandler> logger)
    : IRequestHandler<AddSalesInvoiceItemCommand, Result<SalesInvoiceItemDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<AddSalesInvoiceItemCommandHandler> _logger = logger;

    public async Task<Result<SalesInvoiceItemDto>> Handle(AddSalesInvoiceItemCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.SalesInvoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.SalesInvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Sales invoice with id {InvoiceId} not found.", request.SalesInvoiceId);
            return InvoiceErrors.InvoiceNotFound;
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning("Product with id {ProductId} not found.", request.ProductId);
            return ProductErrors.ProductNotFound;
        }

        var existingIds = invoice.Items.Select(i => i.Id).ToHashSet();
        var addResult = invoice.AddItem(request.ProductId, request.Quantity, request.UnitPrice);
        if (addResult.IsError)
        {
            return addResult.Errors;
        }

        var newItem = invoice.Items.FirstOrDefault(i => !existingIds.Contains(i.Id));
        if (newItem is not null)
        {
            _context.SalesInvoiceItems.Add(newItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(SalesInvoicesCacheKeys.SalesInvoicesListKey, cancellationToken);
        _logger.LogInformation("Item added to sales invoice {InvoiceId}.", invoice.Id);

        return newItem?.ToSalesInvoiceItemDto() ?? invoice.Items.Last().ToSalesInvoiceItemDto();
    }
}
