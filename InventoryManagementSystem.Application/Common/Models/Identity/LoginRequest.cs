using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Application.Common.Models.Identity;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
