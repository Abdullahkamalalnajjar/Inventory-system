using System.Security.Claims;
using InventoryManagementSystem.Application.Common.Models.Identity;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

    Task<Result<TokenResponse>> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
