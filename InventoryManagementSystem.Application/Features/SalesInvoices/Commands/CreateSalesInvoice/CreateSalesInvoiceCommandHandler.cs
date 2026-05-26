using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Application.Features.SalesInvoices.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Invoices;
using InventoryManagementSystem.Domain.Warehouse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Commands.CreateSalesInvoice;

public sealed class CreateSalesInvoiceCommandHandler(
    IApplicationDbContext context,
    HybridCache cache,
    ILogger<CreateSalesInvoiceCommandHandler> logger)
    : IRequestHandler<CreateSalesInvoiceCommand, Result<SalesInvoiceDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CreateSalesInvoiceCommandHandler> _logger = logger;

    public async Task<Result<SalesInvoiceDto>> Handle(CreateSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            _logger.LogWarning("Warehouse with id {WarehouseId} not found.", request.WarehouseId);
            return WarehouseErrors.WarehouseNotFound;
        }

        var numberExists = await _context.SalesInvoices
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceNumber == request.InvoiceNumber, cancellationToken);

        if (numberExists)
        {
            _logger.LogWarning("Sales invoice number {InvoiceNumber} already exists.", request.InvoiceNumber);
            return InvoiceErrors.InvoiceNumberExists;
        }

        var createResult = SalesInvoice.Create(
            request.InvoiceNumber,
            request.WarehouseId,
            request.CustomerId,
            request.InvoiceDateUtc);

        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        _context.SalesInvoices.Add(createResult.Value);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(SalesInvoicesCacheKeys.SalesInvoicesListKey, cancellationToken);
        _logger.LogInformation("Sales invoice created. Id: {InvoiceId}", createResult.Value.Id);

        return createResult.Value.ToSalesInvoiceDto();
    }
}
