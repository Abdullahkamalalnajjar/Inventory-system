using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystem.Infrastructure.Identity;

public sealed class AppUser : IdentityUser
{
    public string? City { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }
}
