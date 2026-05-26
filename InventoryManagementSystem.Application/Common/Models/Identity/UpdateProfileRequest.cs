using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Application.Common.Models.Identity;

public sealed class UpdateProfileRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? PhoneNumber { get; set; }
}
