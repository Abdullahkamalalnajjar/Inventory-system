namespace InventoryManagementSystem.Application.Common.Models.Identity;

public sealed class TokenResponse
{
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime ExpiresOnUtc { get; set; }
}
