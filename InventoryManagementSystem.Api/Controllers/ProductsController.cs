using Asp.Versioning;
using InventoryManagementSystem.Api.Requests.Products;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Application.Features.Products.Commands.CreateProduct;
using InventoryManagementSystem.Application.Features.Products.Commands.DeleteProduct;
using InventoryManagementSystem.Application.Features.Products.Commands.UpdateProduct;
using InventoryManagementSystem.Application.Features.Products.Dtos;
using InventoryManagementSystem.Application.Features.Products.Queries.GetProductById;
using InventoryManagementSystem.Application.Features.Products.Queries.GetProducts;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/v{version:apiVersion}/products")]
[ApiVersion("1.0")]
public sealed class ProductsController(ISender sender) : ApiController
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ProductsRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get Products List")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointName("GetProducts")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductsQuery(), cancellationToken);
        return result.Match(
            onValue: products =>
            {
                Result<List<ProductDto>> response = products;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{productId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProductsRead)]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetProductById")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get Product by Id")]
    [EndpointDescription("Returns detailed information about the product if found.")]
    public async Task<IActionResult> GetProductById([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(productId);
        var result = await _sender.Send(query, cancellationToken);
        return result.Match(
            onValue: product =>
            {
                Result<ProductDto> response = product;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            Problem);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ProductsWrite)]
    [EndpointSummary("Creates a new product.")]
    [EndpointDescription("Adds a new product to the system.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateProductCommand(
                request.Name,
                request.Description,
                request.CategoryId,
                request.UnitId),
            cancellationToken);

        return result.Match(
            onValue: product =>
            {
                Result<ProductDto> response = product;
                return StatusCode(StatusCodes.Status201Created, response);
            },
            onError: Problem);
    }

    [HttpPut("{productId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProductsWrite)]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateProductCommand(
                productId,
                request.Name,
                request.Description,
                request.CategoryId,
                request.UnitId,
                request.Price),
            cancellationToken);

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{productId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProductsWrite)]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProductCommand(productId), cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }
}
