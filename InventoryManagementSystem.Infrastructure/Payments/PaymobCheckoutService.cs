using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Common.Models.Payments;

namespace InventoryManagementSystem.Infrastructure.Payments;

public sealed class PaymobCheckoutService(
    HttpClient httpClient,
    IOptions<PaymobOptions> options,
    ILogger<PaymobCheckoutService> logger) : IPaymentCheckoutService
{
    private readonly PaymobOptions paymobOptions = options.Value;

    public async Task<PaymentCheckoutSessionResponse> CreateSessionAsync(
        PaymentCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(paymobOptions.IframeId))
        {
            return await CreateIframeSessionAsync(request, cancellationToken);
        }

        ValidateCheckoutOptions();

        var integrationIds = request.PaymentMethodIntegrationIds.Count == 0
            ? paymobOptions.PaymentMethodIntegrationIds
            : request.PaymentMethodIntegrationIds.ToArray();

        if (integrationIds.Length == 0)
        {
            throw new InvalidOperationException("Paymob payment method integration ids are not configured.");
        }

        var payload = new PaymobCreateIntentionRequest(
            Amount: request.AmountInMinorUnits,
            Currency: string.IsNullOrWhiteSpace(request.Currency) ? paymobOptions.Currency : request.Currency,
            PaymentMethods: integrationIds,
            Items: request.Items.Select(item => new PaymobItem(
                item.Name,
                item.AmountInMinorUnits,
                item.Description,
                item.Quantity)).ToArray(),
            BillingData: new PaymobBillingData(
                request.BillingData.FirstName,
                request.BillingData.LastName,
                request.BillingData.Email,
                request.BillingData.PhoneNumber,
                request.BillingData.Apartment,
                request.BillingData.Floor,
                request.BillingData.Street,
                request.BillingData.Building,
                request.BillingData.City,
                request.BillingData.State,
                request.BillingData.Country,
                request.BillingData.PostalCode),
            SpecialReference: request.ReferenceCode,
            NotificationUrl: ResolveOptionalUrl(request.NotificationUrl, paymobOptions.NotificationUrl),
            RedirectionUrl: ResolveOptionalUrl(request.RedirectionUrl, paymobOptions.RedirectionUrl),
            Extras: request.Extras);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri());
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Authorization = BuildAuthorizationHeader();
        httpRequest.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Paymob create intention failed. StatusCode={StatusCode}, Response={Response}", (int)response.StatusCode, responseBody);
            throw new InvalidOperationException("Paymob intention request failed.");
        }

        var paymobResponse = JsonSerializer.Deserialize<PaymobCreateIntentionResponse>(responseBody);
        if (paymobResponse is null || string.IsNullOrWhiteSpace(paymobResponse.ClientSecret))
        {
            logger.LogError("Paymob create intention returned an invalid response. Response={Response}", responseBody);
            throw new InvalidOperationException("Paymob intention response is invalid.");
        }

        return new PaymentCheckoutSessionResponse(
            IntentionId: paymobResponse.Id?.ToString() ?? request.ResourceId.ToString(),
            ClientSecret: paymobResponse.ClientSecret,
            PublicKey: paymobOptions.PublicKey);
    }

    private async Task<PaymentCheckoutSessionResponse> CreateIframeSessionAsync(
        PaymentCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        ValidateIframeOptions();

        var integrationIds = request.PaymentMethodIntegrationIds.Count == 0
            ? paymobOptions.PaymentMethodIntegrationIds
            : request.PaymentMethodIntegrationIds.ToArray();

        if (integrationIds.Length == 0)
        {
            throw new InvalidOperationException("Paymob payment method integration ids are not configured.");
        }

        var apiKey = ResolveApiKey();
        var authToken = await CreateAuthTokenAsync(apiKey, cancellationToken);
        var orderId = await CreateOrderAsync(request, authToken, cancellationToken);
        var paymentToken = await CreatePaymentKeyAsync(request, authToken, orderId, integrationIds[0], cancellationToken);
        var iframeUrl = BuildIframeUrl(paymentToken);

        return new PaymentCheckoutSessionResponse(
            IntentionId: orderId,
            ClientSecret: null,
            PublicKey: null,
            PaymentPageUrl: iframeUrl);
    }

    private void ValidateCheckoutOptions()
    {
        if (string.IsNullOrWhiteSpace(paymobOptions.BaseUrl))
        {
            throw new InvalidOperationException("Paymob BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(paymobOptions.SecretKey))
        {
            throw new InvalidOperationException("Paymob SecretKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(paymobOptions.PublicKey))
        {
            throw new InvalidOperationException("Paymob PublicKey is not configured.");
        }
    }

    private void ValidateIframeOptions()
    {
        if (string.IsNullOrWhiteSpace(paymobOptions.BaseUrl))
        {
            throw new InvalidOperationException("Paymob BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(ResolveApiKey()))
        {
            throw new InvalidOperationException("Paymob ApiKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(paymobOptions.IframeId))
        {
            throw new InvalidOperationException("Paymob IframeId is not configured.");
        }
    }

    private static string? ResolveOptionalUrl(string? requestUrl, string? configuredUrl)
        => !string.IsNullOrWhiteSpace(requestUrl) ? requestUrl : string.IsNullOrWhiteSpace(configuredUrl) ? null : configuredUrl;

    private Uri BuildRequestUri()
    {
        var baseUrl = paymobOptions.BaseUrl.TrimEnd('/');
        var path = paymobOptions.IntentionEndpointPath.TrimStart('/');
        return new Uri($"{baseUrl}/{path}", UriKind.Absolute);
    }

    private Uri BuildUri(string path)
    {
        var baseUrl = paymobOptions.BaseUrl.TrimEnd('/');
        return new Uri($"{baseUrl}/{path.TrimStart('/')}", UriKind.Absolute);
    }

    private AuthenticationHeaderValue BuildAuthorizationHeader()
    {
        var prefix = paymobOptions.AuthorizationPrefix?.Trim();
        return string.IsNullOrWhiteSpace(prefix)
            ? new AuthenticationHeaderValue("Bearer", paymobOptions.SecretKey)
            : new AuthenticationHeaderValue(prefix, paymobOptions.SecretKey);
    }

    private string ResolveApiKey()
        => string.IsNullOrWhiteSpace(paymobOptions.ApiKey) ? paymobOptions.SecretKey : paymobOptions.ApiKey;

    private async Task<string> CreateAuthTokenAsync(string apiKey, CancellationToken cancellationToken)
    {
        var payload = new PaymobAuthTokenRequest(apiKey);

        using var response = await httpClient.PostAsJsonAsync(
            BuildUri(paymobOptions.AuthTokenEndpointPath),
            payload,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Paymob auth token request failed. StatusCode={StatusCode}, Response={Response}", (int)response.StatusCode, responseBody);
            throw new InvalidOperationException("Paymob auth token request failed.");
        }

        var authResponse = JsonSerializer.Deserialize<PaymobAuthTokenResponse>(responseBody);
        if (authResponse is null || string.IsNullOrWhiteSpace(authResponse.Token))
        {
            throw new InvalidOperationException("Paymob auth token response is invalid.");
        }

        return authResponse.Token;
    }

    private async Task<string> CreateOrderAsync(
        PaymentCheckoutSessionRequest request,
        string authToken,
        CancellationToken cancellationToken)
    {
        var payload = new PaymobCreateOrderRequest(
            AuthToken: authToken,
            DeliveryNeeded: false,
            AmountCents: request.AmountInMinorUnits.ToString(),
            Currency: string.IsNullOrWhiteSpace(request.Currency) ? paymobOptions.Currency : request.Currency,
            MerchantOrderId: request.ReferenceCode,
            Items: request.Items.Select(item => new PaymobOrderItem(
                item.Name,
                item.AmountInMinorUnits.ToString(),
                item.Description,
                item.Quantity.ToString())).ToArray());

        using var response = await httpClient.PostAsJsonAsync(
            BuildUri(paymobOptions.OrderEndpointPath),
            payload,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Paymob order creation failed. StatusCode={StatusCode}, Response={Response}", (int)response.StatusCode, responseBody);
            throw new InvalidOperationException("Paymob order creation failed.");
        }

        var orderResponse = JsonSerializer.Deserialize<PaymobCreateOrderResponse>(responseBody);
        if (orderResponse is null || orderResponse.Id <= 0)
        {
            throw new InvalidOperationException("Paymob order response is invalid.");
        }

        return orderResponse.Id.ToString();
    }

    private async Task<string> CreatePaymentKeyAsync(
        PaymentCheckoutSessionRequest request,
        string authToken,
        string orderId,
        int integrationId,
        CancellationToken cancellationToken)
    {
        var billingData = new PaymobPaymentKeyBillingData(
            request.BillingData.Apartment,
            request.BillingData.Email,
            request.BillingData.Floor,
            request.BillingData.FirstName,
            request.BillingData.Street,
            request.BillingData.Building,
            request.BillingData.PhoneNumber,
            "PKG",
            request.BillingData.PostalCode,
            request.BillingData.City,
            request.BillingData.Country,
            request.BillingData.LastName,
            request.BillingData.State);

        var payload = new PaymobPaymentKeyRequest(
            AuthToken: authToken,
            AmountCents: request.AmountInMinorUnits.ToString(),
            Expiration: 3600,
            OrderId: orderId,
            BillingData: billingData,
            Currency: string.IsNullOrWhiteSpace(request.Currency) ? paymobOptions.Currency : request.Currency,
            IntegrationId: integrationId,
            LockOrderWhenPaid: true);

        using var response = await httpClient.PostAsJsonAsync(
            BuildUri(paymobOptions.PaymentKeyEndpointPath),
            payload,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Paymob payment key request failed. StatusCode={StatusCode}, Response={Response}", (int)response.StatusCode, responseBody);
            throw new InvalidOperationException("Paymob payment key request failed.");
        }

        var paymentKeyResponse = JsonSerializer.Deserialize<PaymobPaymentKeyResponse>(responseBody);
        if (paymentKeyResponse is null || string.IsNullOrWhiteSpace(paymentKeyResponse.Token))
        {
            throw new InvalidOperationException("Paymob payment key response is invalid.");
        }

        return paymentKeyResponse.Token;
    }

    private string BuildIframeUrl(string paymentToken)
    {
        var baseUrl = paymobOptions.IframeBaseUrl.TrimEnd('/');
        var iframeId = paymobOptions.IframeId?.Trim();
        return $"{baseUrl}/{iframeId}?payment_token={Uri.EscapeDataString(paymentToken)}";
    }

    private sealed record PaymobCreateIntentionRequest(
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("payment_methods")] IReadOnlyCollection<int> PaymentMethods,
        [property: JsonPropertyName("items")] IReadOnlyCollection<PaymobItem> Items,
        [property: JsonPropertyName("billing_data")] PaymobBillingData BillingData,
        [property: JsonPropertyName("special_reference")] string SpecialReference,
        [property: JsonPropertyName("notification_url")] string? NotificationUrl,
        [property: JsonPropertyName("redirection_url")] string? RedirectionUrl,
        [property: JsonPropertyName("extras")] Dictionary<string, string>? Extras);

    private sealed record PaymobItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("quantity")] int Quantity);

    private sealed record PaymobBillingData(
        [property: JsonPropertyName("first_name")] string FirstName,
        [property: JsonPropertyName("last_name")] string LastName,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("phone_number")] string PhoneNumber,
        [property: JsonPropertyName("apartment")] string Apartment,
        [property: JsonPropertyName("floor")] string Floor,
        [property: JsonPropertyName("street")] string Street,
        [property: JsonPropertyName("building")] string Building,
        [property: JsonPropertyName("city")] string City,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("country")] string Country,
        [property: JsonPropertyName("postal_code")] string PostalCode);

    private sealed record PaymobCreateIntentionResponse(
        [property: JsonPropertyName("id")] JsonElementId? Id,
        [property: JsonPropertyName("client_secret")] string? ClientSecret);

    private sealed record PaymobAuthTokenRequest([property: JsonPropertyName("api_key")] string ApiKey);
    private sealed record PaymobAuthTokenResponse([property: JsonPropertyName("token")] string? Token);

    private sealed record PaymobCreateOrderRequest(
        [property: JsonPropertyName("auth_token")] string AuthToken,
        [property: JsonPropertyName("delivery_needed")] bool DeliveryNeeded,
        [property: JsonPropertyName("amount_cents")] string AmountCents,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("merchant_order_id")] string MerchantOrderId,
        [property: JsonPropertyName("items")] IReadOnlyCollection<PaymobOrderItem> Items);

    private sealed record PaymobOrderItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("amount_cents")] string AmountCents,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("quantity")] string Quantity);

    private sealed record PaymobCreateOrderResponse([property: JsonPropertyName("id")] long Id);

    private sealed record PaymobPaymentKeyRequest(
        [property: JsonPropertyName("auth_token")] string AuthToken,
        [property: JsonPropertyName("amount_cents")] string AmountCents,
        [property: JsonPropertyName("expiration")] int Expiration,
        [property: JsonPropertyName("order_id")] string OrderId,
        [property: JsonPropertyName("billing_data")] PaymobPaymentKeyBillingData BillingData,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("integration_id")] int IntegrationId,
        [property: JsonPropertyName("lock_order_when_paid")] bool LockOrderWhenPaid);

    private sealed record PaymobPaymentKeyBillingData(
        [property: JsonPropertyName("apartment")] string Apartment,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("floor")] string Floor,
        [property: JsonPropertyName("first_name")] string FirstName,
        [property: JsonPropertyName("street")] string Street,
        [property: JsonPropertyName("building")] string Building,
        [property: JsonPropertyName("phone_number")] string PhoneNumber,
        [property: JsonPropertyName("shipping_method")] string ShippingMethod,
        [property: JsonPropertyName("postal_code")] string PostalCode,
        [property: JsonPropertyName("city")] string City,
        [property: JsonPropertyName("country")] string Country,
        [property: JsonPropertyName("last_name")] string LastName,
        [property: JsonPropertyName("state")] string State);

    private sealed record PaymobPaymentKeyResponse([property: JsonPropertyName("token")] string? Token);

    [JsonConverter(typeof(JsonElementIdConverter))]
    private sealed record JsonElementId(string Value)
    {
        public override string ToString() => Value;
    }

    private sealed class JsonElementIdConverter : JsonConverter<JsonElementId>
    {
        public override JsonElementId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => new JsonElementId(reader.GetString() ?? string.Empty),
                JsonTokenType.Number => new JsonElementId(reader.GetInt64().ToString()),
                _ => null
            };
        }

        public override void Write(Utf8JsonWriter writer, JsonElementId value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Value);
    }
}
