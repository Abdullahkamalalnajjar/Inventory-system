namespace InventoryManagementSystem.Application.Common.Models.Payments;

public sealed record PaymentCheckoutSessionResponse(
    string IntentionId,
    string? ClientSecret,
    string? PublicKey,
    string? PaymentPageUrl = null);
