using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Application.Features.Categories.Mappers;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCategoriesQueryHandler> logger)
    : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<GetCategoriesQueryHandler> _logger = logger;

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching categories list.");
        var categories = await _context.Categories.AsNoTracking().ToListAsync(cancellationToken);
        return categories.ToCategoryDtoList();
    }
}
