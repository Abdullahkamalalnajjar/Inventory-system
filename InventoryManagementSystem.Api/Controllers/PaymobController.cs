using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Common.Models.Payments;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Api.Controllers;

[AllowAnonymous]
[Route("api/paymob")]
public sealed class PaymobController(IPaymobWebhookService paymobWebhookService) : ApiController
{
    [HttpPost("webhook")]
    [ProducesResponseType(typeof(Result<PaymentWebhookParseResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    public IActionResult Webhook([FromQuery] string? hmac, [FromBody] JsonElement payload)
    {
        var body = payload.GetRawText();
        var resolvedHmac = !string.IsNullOrWhiteSpace(hmac)
            ? hmac
            : payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("hmac", out var hmacProperty)
                ? hmacProperty.GetString()
                : null;

        var result = paymobWebhookService.ParseTransactionCallback(body, resolvedHmac);
        if (!result.IsValid)
        {
            return Problem([Error.Validation("Paymob_Webhook_Invalid", result.FailureReason ?? "Invalid webhook payload.")]);
        }

        Result<PaymentWebhookParseResult> response = result;
        return Ok(response);
    }
}
