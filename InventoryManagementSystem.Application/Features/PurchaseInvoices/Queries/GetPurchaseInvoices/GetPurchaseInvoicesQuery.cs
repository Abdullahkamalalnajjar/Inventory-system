using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoices;

public sealed record GetPurchaseInvoicesQuery() : ICachedQuery<Result<List<PurchaseInvoiceDto>>>
{
    public string CacheKey => PurchaseInvoicesCacheKeys.PurchaseInvoicesListKey;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IEnumerable<string>? Tags => [PurchaseInvoicesCacheKeys.PurchaseInvoicesTag];
}
