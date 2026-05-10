using InventoryManagementSystem.Domain.Common;

namespace InventoryManagementSystem.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset ExpiresOnUtc { get; set; }
}
