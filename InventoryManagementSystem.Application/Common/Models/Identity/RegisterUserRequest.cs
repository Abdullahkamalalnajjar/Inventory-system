using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Application.Common.Models.Identity;

public sealed class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Member";

    public string? City { get; set; }

    public string? PhoneNumber { get; set; }
}
