using Asp.Versioning;
using InventoryManagementSystem.Api.Requests.Categories;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Application.Features.Categories.Commands.CreateCategory;
using InventoryManagementSystem.Application.Features.Categories.Commands.DeleteCategory;
using InventoryManagementSystem.Application.Features.Categories.Commands.UpdateCategory;
using InventoryManagementSystem.Application.Features.Categories.Dtos;
using InventoryManagementSystem.Application.Features.Categories.Queries.GetCategories;
using InventoryManagementSystem.Application.Features.Categories.Queries.GetCategoryById;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/v{version:apiVersion}/categories")]
[ApiVersion("1.0")]
public sealed class CategoriesController(ISender sender) : ApiController
{
    private readonly ISender _sender = sender;
    
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CategoriesRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get Categories List")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointName("GetCategories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _sender.Send(new GetCategoriesQuery());
        return result.Match(
            onValue: category =>
            {
                 Result < List < CategoryDto >> response = (category);
                 return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{categoryId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CategoriesRead)]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetCategoryById")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get Category by Id")]
    [EndpointDescription("Returns details information about the category if found.")]
    public async Task<IActionResult> GetCategoryById([FromRoute] Guid categoryId, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(categoryId);
        var  result = await _sender.Send(query, cancellationToken);
        return result.Match(
            onValue: category =>
            {
                Result<CategoryDto> response = category;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            Problem);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CategoriesWrite)]
    [EndpointSummary("Creates a new category.")]
    [EndpointDescription("Adds a new category to system.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateCategoryCommand(request.Name, request.Description), cancellationToken);

        return result.Match(
            onValue: category =>
            {
                Result<CategoryDto> response = category;
                return StatusCode(StatusCodes.Status201Created, response);
            },
            onError: Problem);
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CategoriesWrite)]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(  [FromRoute] Guid categoryId,[FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateCategoryCommand(categoryId, request.Name, request.Description),
            cancellationToken);

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{categoryId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CategoriesWrite)]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveCategoryCommand(categoryId), cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }

   
}

