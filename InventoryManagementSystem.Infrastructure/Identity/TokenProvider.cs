using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Common.Models.Identity;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Identity;

namespace InventoryManagementSystem.Infrastructure.Identity;

public sealed class TokenProvider(
    IConfiguration configuration,
    IApplicationDbContext context,
    ILogger<TokenProvider> logger,
    IdentityClaimsFactory identityClaimsFactory,
    IIdentityService identityService) : ITokenProvider
{
    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
        => await CreateAsync(user, ct);

    public async Task<Result<TokenResponse>> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Error.Validation("Refresh_Token_Invalid", "Access token and refresh token are required.");
        }

        var principal = GetPrincipalFromExpiredToken(accessToken);
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Error.Unauthorized("Refresh_Token_Invalid", "Access token is invalid.");
        }

        var storedRefreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(token => token.Token == refreshToken && token.UserId == userId, ct);

        if (storedRefreshToken is null || storedRefreshToken.ExpiresOnUtc <= DateTimeOffset.UtcNow)
        {
            return Error.Unauthorized("Refresh_Token_Invalid", "Refresh token is invalid or expired.");
        }

        var userResult = await identityService.GetUserByIdAsync(userId);
        if (userResult.IsError)
        {
            return userResult.Errors;
        }

        context.RefreshTokens.Remove(storedRefreshToken);
        await context.SaveChangesAsync(ct);

        return await CreateAsync(userResult.Value, ct);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["JwtSettings:Audience"],
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogWarning("Expired access token used an invalid security algorithm.");
                return null;
            }

            return principal;
        }
        catch (SecurityTokenException exception)
        {
            logger.LogWarning(exception, "Expired access token validation failed.");
            return null;
        }
    }

    private async Task<Result<TokenResponse>> CreateAsync(AppUserDto user, CancellationToken ct)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var key = jwtSettings["Secret"]!;

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(JwtRegisteredClaimNames.Email, user.Email)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var claim in identityClaimsFactory.Create(user.Roles, user.Claims))
        {
            if (claims.All(existingClaim => existingClaim.Type != claim.Type || existingClaim.Value != claim.Value))
            {
                claims.Add(claim);
            }
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(descriptor);

        await context.RefreshTokens
            .Where(token => token.UserId == user.UserId)
            .ExecuteDeleteAsync(ct);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            UserId = user.UserId,
            ExpiresOnUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(ct);

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(securityToken),
            RefreshToken = refreshToken.Token,
            ExpiresOnUtc = expires
        };
    }
}
