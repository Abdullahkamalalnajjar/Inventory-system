namespace InventoryManagementSystem.Application.Common.Models.Payments;

public sealed record PaymentWebhookParseResult(
    bool IsValid,
    string? ReferenceCode,
    string? ExternalTransactionId,
    bool IsSuccessfulPayment,
    bool IsFailure,
    string? FailureReason,
    string? EventType);
