using Asp.Versioning;
using InventoryManagementSystem.Api.Requests.Units;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Application.Features.Units;
using InventoryManagementSystem.Application.Features.Units.Commands.CreateUnitCommand;
using InventoryManagementSystem.Application.Features.Units.Commands.RemoveUnit;
using InventoryManagementSystem.Application.Features.Units.Commands.UpdateUnit;
using InventoryManagementSystem.Application.Features.Units.Dtos;
using InventoryManagementSystem.Application.Features.Units.Queries.GetUnitById;
using InventoryManagementSystem.Application.Features.Units.Queries.GetUnits;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/v{version:apiVersion}/units")]
[ApiVersion("1.0")]
public sealed class UnitsController(ISender sender, IOutputCacheStore outputCacheStore) : ApiController
{
    private readonly ISender _sender = sender;
    private readonly IOutputCacheStore _outputCacheStore = outputCacheStore;

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.UnitsRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<UnitDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get Units List")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointName("GetUnits")]
   // [OutputCache(PolicyName = "Units")]
    public async Task<IActionResult> GetUnits(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUnitsQuery(), cancellationToken);
        return result.Match(
            onValue: units =>
            {
                Result<List<UnitDto>> response = units;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{unitId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.UnitsRead)]
    [ProducesResponseType(typeof(Result<UnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetUnitById")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get Unit by Id")]
    [EndpointDescription("Returns detailed information about the unit if found.")]
    public async Task<IActionResult> GetUnitById([FromRoute] Guid unitId, CancellationToken cancellationToken)
    {
        var query = new GetUnitByIdQuery(unitId);
        var result = await _sender.Send(query, cancellationToken);
        return result.Match(
            onValue: unit =>
            {
                Result<UnitDto> response = unit;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            Problem);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.UnitsWrite)]
    [EndpointSummary("Creates a new unit.")]
    [EndpointDescription("Adds a new unit to the system.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<UnitDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateUnitCommand(request.UnitName ?? string.Empty, request.Symbol ?? string.Empty),
            cancellationToken);
        
        return result.Match(
            onValue: unit =>
            {
                Result<UnitDto> response = unit;
                return StatusCode(StatusCodes.Status201Created, response);
            },
            onError: Problem);
    }

    [HttpPut("{unitId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.UnitsWrite)]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid unitId,
        [FromBody] UpdateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateUnitCommand(unitId, request.UnitName ?? string.Empty, request.Symbol ?? string.Empty),
            cancellationToken);

        if (result.IsSuccess)
        {
            await _outputCacheStore.EvictByTagAsync(UnitsCacheKeys.UnitsTag, cancellationToken);
        }

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{unitId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.UnitsWrite)]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid unitId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveUnitCommand(unitId), cancellationToken);

        if (result.IsSuccess)
        {
            await _outputCacheStore.EvictByTagAsync(UnitsCacheKeys.UnitsTag, cancellationToken);
        }

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }
}
