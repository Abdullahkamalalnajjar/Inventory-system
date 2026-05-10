using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Common.Models.Payments;

namespace InventoryManagementSystem.Infrastructure.Payments;

public sealed class PaymobWebhookService(IOptions<PaymobOptions> options) : IPaymobWebhookService
{
    private readonly PaymobOptions paymobOptions = options.Value;

    public PaymentWebhookParseResult ParseTransactionCallback(string payloadJson, string? providedHmac)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return Invalid("Paymob callback payload is empty.");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(payloadJson);
        }
        catch (JsonException)
        {
            return Invalid("Paymob callback payload is invalid JSON.");
        }

        using (document)
        {
            var root = document.RootElement;
            var callbackObject = GetCallbackObject(root);
            if (callbackObject.ValueKind == JsonValueKind.Undefined || callbackObject.ValueKind == JsonValueKind.Null)
            {
                return Invalid("Paymob callback object is missing.");
            }

            if (!IsValidHmac(callbackObject, providedHmac))
            {
                return Invalid("Paymob callback HMAC is invalid.");
            }

            var referenceCode = GetReferenceCode(callbackObject);
            var transactionId = GetString(callbackObject, "id");
            var success = GetBoolean(callbackObject, "success");
            var pending = GetBoolean(callbackObject, "pending");
            var errorOccured = GetBoolean(callbackObject, "error_occured");
            var isVoided = GetBoolean(callbackObject, "is_voided") || GetBoolean(callbackObject, "is_void");
            var isRefunded = GetBoolean(callbackObject, "is_refunded") || GetBoolean(callbackObject, "is_refund");

            var isSuccessfulPayment = success && !pending && !errorOccured && !isVoided && !isRefunded;
            var isFailure = !isSuccessfulPayment && (!success || errorOccured || isVoided || isRefunded);

            var eventType = GetString(root, "type");
            var failureReason = isFailure
                ? ResolveFailureReason(callbackObject, success, pending, errorOccured, isVoided, isRefunded)
                : null;

            return new PaymentWebhookParseResult(
                IsValid: true,
                ReferenceCode: referenceCode,
                ExternalTransactionId: transactionId,
                IsSuccessfulPayment: isSuccessfulPayment,
                IsFailure: isFailure,
                FailureReason: failureReason,
                EventType: eventType);
        }
    }

    private bool IsValidHmac(JsonElement callbackObject, string? providedHmac)
    {
        if (string.IsNullOrWhiteSpace(paymobOptions.HmacSecret))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(providedHmac))
        {
            return false;
        }

        var payload = string.Concat(
            NormalizeForHmac(GetString(callbackObject, "amount_cents")),
            NormalizeForHmac(GetString(callbackObject, "created_at")),
            NormalizeForHmac(GetString(callbackObject, "currency")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "error_occured")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "has_parent_transaction")),
            NormalizeForHmac(GetString(callbackObject, "id")),
            NormalizeForHmac(GetString(callbackObject, "integration_id")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "is_3d_secure")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "is_auth")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "is_capture")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "is_refunded")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "is_standalone_payment")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "is_voided")),
            NormalizeForHmac(GetOrderId(callbackObject)),
            NormalizeForHmac(GetString(callbackObject, "owner")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "pending")),
            NormalizeForHmac(GetNestedString(callbackObject, "source_data", "pan") ?? GetString(callbackObject, "source_data_pan")),
            NormalizeForHmac(GetNestedString(callbackObject, "source_data", "sub_type") ?? GetString(callbackObject, "source_data_sub_type")),
            NormalizeForHmac(GetNestedString(callbackObject, "source_data", "type") ?? GetString(callbackObject, "source_data_type")),
            NormalizeForHmac(GetRawBooleanString(callbackObject, "success")));

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(paymobOptions.HmacSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var generated = Convert.ToHexString(hash).ToLowerInvariant();

        return string.Equals(generated, providedHmac.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static JsonElement GetCallbackObject(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("obj", out var obj))
        {
            return obj;
        }

        return root;
    }

    private static string? GetReferenceCode(JsonElement callbackObject)
    {
        var fromExtras = GetNestedString(callbackObject, "extras", "referenceCode")
            ?? GetNestedString(callbackObject, "extras", "reference_code")
            ?? GetNestedString(callbackObject, "extras", "merchant_intention_id");

        if (!string.IsNullOrWhiteSpace(fromExtras))
        {
            return fromExtras;
        }

        var direct = GetString(callbackObject, "special_reference")
            ?? GetString(callbackObject, "merchant_order_id")
            ?? GetNestedString(callbackObject, "order", "merchant_order_id");

        return string.IsNullOrWhiteSpace(direct) ? null : direct;
    }

    private static string ResolveFailureReason(
        JsonElement callbackObject,
        bool success,
        bool pending,
        bool errorOccured,
        bool isVoided,
        bool isRefunded)
    {
        if (pending)
        {
            return "Payment is still pending.";
        }

        if (errorOccured)
        {
            return GetString(callbackObject, "data.message")
                   ?? GetString(callbackObject, "txn_response_code")
                   ?? "Payment failed due to a gateway error.";
        }

        if (isVoided)
        {
            return "Payment was voided.";
        }

        if (isRefunded)
        {
            return "Payment was refunded.";
        }

        if (!success)
        {
            return "Payment was not successful.";
        }

        return "Payment callback reported a failure state.";
    }

    private static string GetOrderId(JsonElement callbackObject)
        => GetNestedString(callbackObject, "order", "id")
           ?? GetString(callbackObject, "order")
           ?? string.Empty;

    private static PaymentWebhookParseResult Invalid(string reason)
        => new(false, null, null, false, true, reason, null);

    private static string NormalizeForHmac(string? value) => value ?? string.Empty;

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (propertyName.Contains('.'))
        {
            var segments = propertyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var current = element;

            foreach (var segment in segments)
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
                {
                    return null;
                }
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Number => current.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => current.ToString()
            };
        }

        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => property.ToString()
        };
    }

    private static string? GetNestedString(JsonElement element, string parent, string child)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(parent, out var parentElement))
        {
            return null;
        }

        if (parentElement.ValueKind != JsonValueKind.Object || !parentElement.TryGetProperty(child, out var childElement))
        {
            return null;
        }

        return childElement.ValueKind switch
        {
            JsonValueKind.String => childElement.GetString(),
            JsonValueKind.Number => childElement.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => childElement.ToString()
        };
    }

    private static string GetRawBooleanString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty
        };
    }

    private static bool GetBoolean(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var parsed) => parsed,
            _ => false
        };
    }
}
