using InventoryManagementSystem.Application.Common.Models.Identity;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string? policyName);

    Task<Result<AppUserDto>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);

    Task<Result<AppUserDto>> GetUserByIdAsync(string userId);

    Task<Result<AppUserDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

    Task<List<AppUserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    Task<List<AppUserDto>> GetDeletedUsersAsync(CancellationToken cancellationToken = default);

    Task<Result<Deleted>> SoftDeleteAsync(string userId, CancellationToken cancellationToken = default);

    Task<Result<Updated>> RestoreDeletedUserAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<string?> GetUserNameAsync(string userId);

    Task<Dictionary<string, string?>> GetUserNamesAsync(List<string> userIds);
}
