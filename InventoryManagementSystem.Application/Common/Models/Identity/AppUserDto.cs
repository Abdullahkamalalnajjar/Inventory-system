using System.Security.Claims;

namespace InventoryManagementSystem.Application.Common.Models.Identity;

public sealed record AppUserDto(
    string UserId,
    string Email,
    IList<string> Roles,
    IList<Claim> Claims,
    string? City = null,
    string? PhoneNumber = null);
