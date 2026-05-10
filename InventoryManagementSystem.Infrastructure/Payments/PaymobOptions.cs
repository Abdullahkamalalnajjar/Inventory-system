namespace InventoryManagementSystem.Infrastructure.Payments;

public sealed class PaymobOptions
{
    public const string SectionName = "Paymob";

    public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
    public string? ApiKey { get; set; }
    public string IntentionEndpointPath { get; set; } = "v1/intention/";
    public string AuthTokenEndpointPath { get; set; } = "auth/tokens";
    public string OrderEndpointPath { get; set; } = "ecommerce/orders";
    public string PaymentKeyEndpointPath { get; set; } = "acceptance/payment_keys";
    public string IframeBaseUrl { get; set; } = "https://accept.paymob.com/api/acceptance/iframes";
    public string? IframeId { get; set; }
    public string AuthorizationPrefix { get; set; } = "Token";
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string HmacSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "EGP";
    public int[] PaymentMethodIntegrationIds { get; set; } = [];
    public string? NotificationUrl { get; set; }
    public string? RedirectionUrl { get; set; }
}
