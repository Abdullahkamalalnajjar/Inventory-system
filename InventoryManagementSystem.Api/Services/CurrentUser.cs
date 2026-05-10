using System.Security.Claims;
using InventoryManagementSystem.Application.Common.Interfaces;

namespace InventoryManagementSystem.Api.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
