using Asp.Versioning;
using InventoryManagementSystem.Api.Requests.Warehouses;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Application.Features.Warehouses.Commands.CreateWarehouse;
using InventoryManagementSystem.Application.Features.Warehouses.Commands.DeleteWarehouse;
using InventoryManagementSystem.Application.Features.Warehouses.Commands.UpdateWarehouse;
using InventoryManagementSystem.Application.Features.Warehouses.Dtos;
using InventoryManagementSystem.Application.Features.Warehouses.Queries.GetWarehouses;
using InventoryManagementSystem.Application.Features.Warehouses.Queries.GetWarehouseById;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/v{version:apiVersion}/warehouses")]
[ApiVersion("1.0")]
public sealed class WarehousesController(ISender sender) : ApiController
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.WarehousesRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<WarehouseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get Warehouses List")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointName("GetWarehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        var result = await _sender.Send(new GetWarehousesQuery());
        return result.Match(
            onValue: warehouses =>
            {
                Result<List<WarehouseDto>> response = warehouses;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{warehouseId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WarehousesRead)]
    [ProducesResponseType(typeof(Result<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetWarehouseById")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get Warehouse by Id")]
    [EndpointDescription("Returns detailed information about the warehouse if found.")]
    public async Task<IActionResult> GetWarehouseById([FromRoute] Guid warehouseId, CancellationToken cancellationToken)
    {
        var query = new GetWarehouseByIdQuery(warehouseId);
        var result = await _sender.Send(query, cancellationToken);
        return result.Match(
            onValue: warehouse =>
            {
                Result<WarehouseDto> response = warehouse;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            Problem);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.WarehousesWrite)]
    [EndpointSummary("Creates a new warehouse.")]
    [EndpointDescription("Adds a new warehouse to the system.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<WarehouseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateWarehouseCommand(request.Name, request.Address), cancellationToken);

        return result.Match(
            onValue: warehouse =>
            {
                Result<WarehouseDto> response = warehouse;
                return StatusCode(StatusCodes.Status201Created, response);
            },
            onError: Problem);
    }

    [HttpPut("{warehouseId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WarehousesWrite)]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] Guid warehouseId, [FromBody] UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateWarehouseCommand(warehouseId, request.Name, request.Address),
            cancellationToken);

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{warehouseId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WarehousesWrite)]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid warehouseId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveWarehouseCommand(warehouseId), cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }
}
