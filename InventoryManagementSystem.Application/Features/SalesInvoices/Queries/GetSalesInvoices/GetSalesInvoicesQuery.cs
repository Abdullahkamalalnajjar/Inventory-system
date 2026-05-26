using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;

namespace InventoryManagementSystem.Application.Features.SalesInvoices.Queries.GetSalesInvoices;

public sealed record GetSalesInvoicesQuery() : ICachedQuery<Result<List<SalesInvoiceDto>>>
{
    public string CacheKey => SalesInvoicesCacheKeys.SalesInvoicesListKey;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IEnumerable<string>? Tags => [SalesInvoicesCacheKeys.SalesInvoicesTag];
}
