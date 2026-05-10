using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Common.Models.Payments;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Api.Controllers;

[Route("api/payments")]
[Authorize(Policy = AuthorizationPolicies.PaymentsCheckout)]
public sealed class PaymentsController(IPaymentCheckoutService paymentCheckoutService) : ApiController
{
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(Result<PaymentCheckoutSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] PaymentCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await paymentCheckoutService.CreateSessionAsync(request, cancellationToken);
            Result<PaymentCheckoutSessionResponse> response = session;
            return Ok(response);
        }
        catch (Exception exception)
        {
            return Problem([Error.Unexpected("Payment_Checkout_Failed", exception.Message)]);
        }
    }
}
