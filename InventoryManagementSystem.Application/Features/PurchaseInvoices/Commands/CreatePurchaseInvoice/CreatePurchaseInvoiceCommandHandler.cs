using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Warehouse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice;

public sealed class CreatePurchaseInvoiceCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreatePurchaseInvoiceCommandHandler> logger)
    : IRequestHandler<CreatePurchaseInvoiceCommand, Result<PurchaseInvoiceDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreatePurchaseInvoiceCommandHandler> _logger = logger;

    public async Task<Result<PurchaseInvoiceDto>> Handle(CreatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            _logger.LogWarning("Warehouse with id {WarehouseId} not found.", request.WarehouseId);
            return WarehouseErrors.WarehouseNotFound;
        }

        var numberExists = await _context.PurchaseInvoices
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceNumber == request.InvoiceNumber, cancellationToken);

        if (numberExists)
        {
            _logger.LogWarning("Purchase invoice number {InvoiceNumber} already exists.", request.InvoiceNumber);
            return InvoiceErrors.InvoiceNumberExists;
        }

        var createResult = PurchaseInvoice.Create(
            request.InvoiceNumber,
            request.WarehouseId,
            request.SupplierId,
            request.InvoiceDateUtc);

        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        _context.PurchaseInvoices.Add(createResult.Value);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(PurchaseInvoicesCacheKeys.PurchaseInvoicesListKey, cancellationToken);
        _logger.LogInformation("Purchase invoice created. Id: {InvoiceId}", createResult.Value.Id);

        return createResult.Value.ToPurchaseInvoiceDto();
    }
}
