namespace InventoryManagementSystem.Infrastructure.Data.Seed;

public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public bool Enabled { get; set; } = true;

    public string AdminEmail { get; set; } = "admin@example.com";

    public string AdminPassword { get; set; } = "Admin123!";

    public string AdminRole { get; set; } = "Manager";

    public string? AdminCity { get; set; } = "Cairo";

    public string? AdminPhoneNumber { get; set; } = "+201000000000";
}
