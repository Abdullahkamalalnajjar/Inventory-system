using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Application.Common.Models.Identity;

public sealed class RefreshTokenRequest
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
