using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Application.Common.Models.Payments;

public sealed class PaymentCheckoutSessionRequest
{
    [Required]
    public Guid ResourceId { get; set; }

    [Required]
    public string ReferenceCode { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long AmountInMinorUnits { get; set; }

    [Required]
    public string Currency { get; set; } = "EGP";

    public IReadOnlyCollection<int> PaymentMethodIntegrationIds { get; set; } = [];

    [Required]
    public PaymentBillingData BillingData { get; set; } = new();

    public IReadOnlyCollection<PaymentItem> Items { get; set; } = [];

    public string? NotificationUrl { get; set; }

    public string? RedirectionUrl { get; set; }

    public Dictionary<string, string>? Extras { get; set; }
}

public sealed class PaymentBillingData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Apartment { get; set; } = "NA";
    public string Floor { get; set; } = "NA";
    public string Street { get; set; } = "NA";
    public string Building { get; set; } = "NA";
    public string City { get; set; } = "NA";
    public string State { get; set; } = "NA";
    public string Country { get; set; } = "EG";
    public string PostalCode { get; set; } = "NA";
}

public sealed class PaymentItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long AmountInMinorUnits { get; set; }
    public int Quantity { get; set; } = 1;
}
