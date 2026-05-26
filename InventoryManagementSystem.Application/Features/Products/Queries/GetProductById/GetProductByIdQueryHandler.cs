using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Application.Features.Products.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Product;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IApplicationDbContext context,ILogger<GetProductByIdQueryHandler> logger)
:IRequestHandler<GetProductByIdQuery,Result<ProductDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetProductByIdQueryHandler> _logger = logger;
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product with Id:{ProductId} not found", request.Id);
            return ProductErrors.ProductNotFound;
        }

        var totalQuantity = await _context.StockItems
            .AsNoTracking()
            .Where(stockItem => stockItem.ProductId == request.Id)
            .SumAsync(stockItem => stockItem.QuantityOnHand, cancellationToken);

        return product.ToProductDto(totalQuantity);
    }
}
