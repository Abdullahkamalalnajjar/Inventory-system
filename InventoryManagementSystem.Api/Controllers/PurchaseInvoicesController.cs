using Asp.Versioning;
using InventoryManagementSystem.Api.Requests.PurchaseInvoices;
using InventoryManagementSystem.Api.Services;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.AddPurchaseInvoiceItem;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.DeletePurchaseInvoice;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.PostPurchaseInvoice;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Commands.RemovePurchaseInvoiceItem;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoices;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/v{version:apiVersion}/purchase-invoices")]
[ApiVersion("1.0")]
public sealed class PurchaseInvoicesController(ISender sender) : ApiController
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<PurchaseInvoiceDto>>), StatusCodes.Status200OK)]
    [EndpointSummary("Get Purchase Invoices List")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetPurchaseInvoices(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPurchaseInvoicesQuery(), cancellationToken);
        return result.Match(
            onValue: invoices =>
            {
                Result<List<PurchaseInvoiceDto>> response = invoices;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{purchaseInvoiceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesRead)]
    [ProducesResponseType(typeof(Result<PurchaseInvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get Purchase Invoice by Id")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetPurchaseInvoiceById([FromRoute] Guid purchaseInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPurchaseInvoiceByIdQuery(purchaseInvoiceId), cancellationToken);
        return result.Match(
            onValue: invoice =>
            {
                Result<PurchaseInvoiceDto> response = invoice;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{purchaseInvoiceId:guid}/print")]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesRead)]
    [Produces("text/html")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Print Purchase Invoice")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Print([FromRoute] Guid purchaseInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPurchaseInvoiceByIdQuery(purchaseInvoiceId), cancellationToken);
        return result.Match<IActionResult>(
            onValue: invoice => Content(InvoicePrintHtmlBuilder.BuildPurchaseInvoice(invoice), "text/html"),
            onError: Problem);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesWrite)]
    [EndpointSummary("Creates a new purchase invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<PurchaseInvoiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreatePurchaseInvoiceCommand(request.InvoiceNumber, request.WarehouseId, request.SupplierId, request.InvoiceDateUtc),
            cancellationToken);

        return result.Match(
            onValue: invoice =>
            {
                Result<PurchaseInvoiceDto> response = invoice;
                return StatusCode(StatusCodes.Status201Created, response);
            },
            onError: Problem);
    }

    [HttpPost("{purchaseInvoiceId:guid}/items")]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesWrite)]
    [EndpointSummary("Adds an item to a purchase invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<PurchaseInvoiceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddItem(
        [FromRoute] Guid purchaseInvoiceId,
        [FromBody] AddPurchaseInvoiceItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddPurchaseInvoiceItemCommand(purchaseInvoiceId, request.ProductId, request.Quantity, request.UnitCost),
            cancellationToken);

        return result.Match(
            onValue: item =>
            {
                Result<PurchaseInvoiceItemDto> response = item;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{purchaseInvoiceId:guid}/items/{purchaseInvoiceItemId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesWrite)]
    [EndpointSummary("Removes an item from a draft purchase invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveItem(
        [FromRoute] Guid purchaseInvoiceId,
        [FromRoute] Guid purchaseInvoiceItemId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RemovePurchaseInvoiceItemCommand(purchaseInvoiceId, purchaseInvoiceItemId),
            cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpPost("{purchaseInvoiceId:guid}/post")]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesWrite)]
    [EndpointSummary("Posts a purchase invoice and updates stock.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromRoute] Guid purchaseInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PostPurchaseInvoiceCommand(purchaseInvoiceId), cancellationToken);

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{purchaseInvoiceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.PurchaseInvoicesWrite)]
    [EndpointSummary("Deletes a draft purchase invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid purchaseInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeletePurchaseInvoiceCommand(purchaseInvoiceId), cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }
}
