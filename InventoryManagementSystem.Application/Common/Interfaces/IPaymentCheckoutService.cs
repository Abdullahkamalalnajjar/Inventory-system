using InventoryManagementSystem.Application.Common.Models.Payments;

namespace InventoryManagementSystem.Application.Common.Interfaces;

public interface IPaymentCheckoutService
{
    Task<PaymentCheckoutSessionResponse> CreateSessionAsync(
        PaymentCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);
}
