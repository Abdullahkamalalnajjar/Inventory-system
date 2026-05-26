using Asp.Versioning;
using InventoryManagementSystem.Api.Requests.SalesInvoices;
using InventoryManagementSystem.Api.Services;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Application.Features.SalesInvoices.Commands.AddSalesInvoiceItem;
using InventoryManagementSystem.Application.Features.SalesInvoices.Commands.CreateSalesInvoice;
using InventoryManagementSystem.Application.Features.SalesInvoices.Commands.DeleteSalesInvoice;
using InventoryManagementSystem.Application.Features.SalesInvoices.Commands.PostSalesInvoice;
using InventoryManagementSystem.Application.Features.SalesInvoices.Commands.RemoveSalesInvoiceItem;
using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;
using InventoryManagementSystem.Application.Features.SalesInvoices.Queries.GetSalesInvoiceById;
using InventoryManagementSystem.Application.Features.SalesInvoices.Queries.GetSalesInvoices;
using InventoryManagementSystem.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/v{version:apiVersion}/sales-invoices")]
[ApiVersion("1.0")]
public sealed class SalesInvoicesController(ISender sender) : ApiController
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesRead)]
    [ProducesResponseType(typeof(Result<IEnumerable<SalesInvoiceDto>>), StatusCodes.Status200OK)]
    [EndpointSummary("Get Sales Invoices List")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetSalesInvoices(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSalesInvoicesQuery(), cancellationToken);
        return result.Match(
            onValue: invoices =>
            {
                Result<List<SalesInvoiceDto>> response = invoices;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{salesInvoiceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesRead)]
    [ProducesResponseType(typeof(Result<SalesInvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get Sales Invoice by Id")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetSalesInvoiceById([FromRoute] Guid salesInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSalesInvoiceByIdQuery(salesInvoiceId), cancellationToken);
        return result.Match(
            onValue: invoice =>
            {
                Result<SalesInvoiceDto> response = invoice;
                return StatusCode(StatusCodes.Status200OK, response);
            },
            onError: Problem);
    }

    [HttpGet("{salesInvoiceId:guid}/print")]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesRead)]
    [Produces("text/html")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Print Sales Invoice")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Print([FromRoute] Guid salesInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSalesInvoiceByIdQuery(salesInvoiceId), cancellationToken);
        return result.Match<IActionResult>(
            onValue: invoice => Content(InvoicePrintHtmlBuilder.BuildSalesInvoice(invoice), "text/html"),
            onError: Problem);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesWrite)]
    [EndpointSummary("Creates a new sales invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<SalesInvoiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateSalesInvoiceCommand(request.InvoiceNumber, request.WarehouseId, request.CustomerId, request.InvoiceDateUtc),
            cancellationToken);

        return result.Match(
            onValue: invoice =>
            {
                Result<SalesInvoiceDto> response = invoice;
                return StatusCode(StatusCodes.Status201Created, response);
            },
            onError: Problem);
    }

    [HttpPost("{salesInvoiceId:guid}/items")]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesWrite)]
    [EndpointSummary("Adds an item to a sales invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<SalesInvoiceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddItem(
        [FromRoute] Guid salesInvoiceId,
        [FromBody] AddSalesInvoiceItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddSalesInvoiceItemCommand(salesInvoiceId, request.ProductId, request.Quantity, request.UnitPrice),
            cancellationToken);

        return result.Match(
            onValue: item =>
            {
                Result<SalesInvoiceItemDto> response = item;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{salesInvoiceId:guid}/items/{salesInvoiceItemId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesWrite)]
    [EndpointSummary("Removes an item from a draft sales invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveItem(
        [FromRoute] Guid salesInvoiceId,
        [FromRoute] Guid salesInvoiceItemId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RemoveSalesInvoiceItemCommand(salesInvoiceId, salesInvoiceItemId),
            cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpPost("{salesInvoiceId:guid}/post")]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesWrite)]
    [EndpointSummary("Posts a sales invoice and updates stock.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromRoute] Guid salesInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PostSalesInvoiceCommand(salesInvoiceId), cancellationToken);

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{salesInvoiceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SalesInvoicesWrite)]
    [EndpointSummary("Deletes a draft sales invoice.")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] Guid salesInvoiceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteSalesInvoiceCommand(salesInvoiceId), cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }
}
