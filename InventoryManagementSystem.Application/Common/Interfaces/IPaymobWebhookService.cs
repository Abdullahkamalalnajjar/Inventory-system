using InventoryManagementSystem.Application.Common.Models.Payments;

namespace InventoryManagementSystem.Application.Common.Interfaces;

public interface IPaymobWebhookService
{
    PaymentWebhookParseResult ParseTransactionCallback(string payloadJson, string? providedHmac);
}
