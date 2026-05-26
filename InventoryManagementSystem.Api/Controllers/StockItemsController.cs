using Asp.Versioning;
using InventoryManagementSystem.Api.Requests.StockItems;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Application.Features.Stock.Commands.CreateStockItem;
using InventoryManagementSystem.Application.Features.Stock.Commands.DeleteStockItem;
using InventoryManagementSystem.Application.Features.Stock.Commands.UpdateStockItem;
using InventoryManagementSystem.Application.Features.Stock.Dtos;
using InventoryManagementSystem.Application.Features.Stock.Queries.GetStockItems;
using InventoryManagementSystem.Application.Features.Stock.Queries.GetStockItemById;
using InventoryManagementSystem.Application.Features.Stock.Queries.GetStockMovements;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/v{version:apiVersion}/stock-items")]
[ApiVersion("1.0")]
public sealed class StockItemsController(ISender sender) : ApiController
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.StockRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<StockItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get Stock Items List")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointName("GetStockItems")]
    public async Task<IActionResult> GetStockItems(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockItemsQuery(warehouseId, productId), cancellationToken);
        return result.Match(
            onValue: items =>
            {
                Result<List<StockItemDto>> response = items;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{stockItemId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StockRead)]
    [ProducesResponseType(typeof(Result<StockItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointName("GetStockItemById")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get Stock Item by Id")]
    public async Task<IActionResult> GetStockItemById([FromRoute] Guid stockItemId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockItemByIdQuery(stockItemId), cancellationToken);
        return result.Match(
            onValue: item =>
            {
                Result<StockItemDto> response = item;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{stockItemId:guid}/movements")]
    [Authorize(Policy = AuthorizationPolicies.StockRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<StockMovementDto>>), StatusCodes.Status200OK)]
    [EndpointName("GetStockItemMovements")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get Stock Movements for Stock Item")]
    public async Task<IActionResult> GetStockItemMovements([FromRoute] Guid stockItemId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockMovementsQuery(StockItemId: stockItemId), cancellationToken);
        return result.Match(
            onValue: movements =>
            {
                Result<List<StockMovementDto>> response = movements;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("movements")]
    [Authorize(Policy = AuthorizationPolicies.StockRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<StockMovementDto>>), StatusCodes.Status200OK)]
    [EndpointName("GetStockMovements")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get Stock Movements")]
    public async Task<IActionResult> GetStockMovements(
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockMovementsQuery(ProductId: productId, WarehouseId: warehouseId), cancellationToken);
        return result.Match(
            onValue: movements =>
            {
                Result<List<StockMovementDto>> response = movements;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.StockWrite)]
    [EndpointSummary("Creates a new stock item.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<StockItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateStockItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateStockItemCommand(request.ProductId, request.WarehouseId, request.InitialQuantity),
            cancellationToken);

        return result.Match(
            onValue: item =>
            {
                Result<StockItemDto> response = item;
                return StatusCode(StatusCodes.Status201Created, response);
            },
            onError: Problem);
    }

    [HttpPut("{stockItemId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StockWrite)]
    [EndpointSummary("Adjusts stock item quantity.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid stockItemId,
        [FromBody] UpdateStockItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateStockItemCommand(stockItemId, request.NewQuantity, request.ReferenceNumber, request.Notes),
            cancellationToken);

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{stockItemId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StockWrite)]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid stockItemId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveStockItemCommand(stockItemId), cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }
}
