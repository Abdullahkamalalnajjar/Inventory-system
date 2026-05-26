using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Common.Models.Identity;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Api.Controllers;

[Route("identity")]
[ApiVersionNeutral]
public sealed class AuthController(IIdentityService identityService, ITokenProvider tokenProvider) : ApiController
{
    private const string ForceLogoutHeaderName = "X-Force-Logout";

    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<TokenResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var userResult = await identityService.RegisterAsync(request, cancellationToken);
        if (userResult.IsError)
        {
            return Problem(userResult.Errors);
        }

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, cancellationToken);
        if (tokenResult.IsError)
        {
            return Problem(tokenResult.Errors);
        }

        Result<TokenResponse> response = tokenResult.Value;
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("token/generate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateToken([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var userResult = await identityService.AuthenticateAsync(request.Email, request.Password);
        if (userResult.IsError)
        {
            return Problem(userResult.Errors);
        }

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, cancellationToken);
        if (tokenResult.IsError)
        {
            return Problem(tokenResult.Errors);
        }

        Result<TokenResponse> response = tokenResult.Value;
        return Ok(response);
    }

    [HttpPost("token/refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await tokenProvider.RefreshTokenAsync(request.AccessToken, request.RefreshToken, cancellationToken);

        return result.Match(
            onValue: token =>
            {
                Result<TokenResponse> response = token;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpPost("restore-deleted-user")]
    [Authorize(Policy = AuthorizationPolicies.UsersWrite)]
    [ProducesResponseType(typeof(Result<Updated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RestoreDeletedUser([FromBody] RestoreDeletedUserRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RestoreDeletedUserAsync(request.Email, request.Password, cancellationToken);

        return result.Match(
            onValue: updated =>
            {
                Result<Updated> response = updated;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpGet("current-user")]
    [Authorize(Policy = AuthorizationPolicies.CurrentUserRead)]
    [ProducesResponseType(typeof(Result<AppUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem([Error.Unauthorized("Invalid_Token", "Authenticated user identifier is missing.")]);
        }

        var result = await identityService.GetUserByIdAsync(userId);

        return result.Match(
            onValue: user =>
            {
                Result<AppUserDto> response = user;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpPut("current-user/profile")]
    [Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(Result<AppUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem([Error.Unauthorized("Invalid_Token", "Authenticated user identifier is missing.")]);
        }

        var result = await identityService.UpdateProfileAsync(userId, request, cancellationToken);

        return result.Match(
            onValue: user =>
            {
                Result<AppUserDto> response = user;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpGet("users")]
    [Authorize(Policy = AuthorizationPolicies.UsersRead)]
    [ProducesResponseType(typeof(Result<List<AppUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActiveUsers(CancellationToken cancellationToken)
    {
        var users = await identityService.GetActiveUsersAsync(cancellationToken);
        Result<List<AppUserDto>> response = users;
        return Ok(response);
    }

    [HttpGet("users/deleted")]
    [Authorize(Policy = AuthorizationPolicies.UsersRead)]
    [ProducesResponseType(typeof(Result<List<AppUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDeletedUsers(CancellationToken cancellationToken)
    {
        var users = await identityService.GetDeletedUsersAsync(cancellationToken);
        Result<List<AppUserDto>> response = users;
        return Ok(response);
    }

    [HttpDelete("current-user")]
    [Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Problem([Error.Unauthorized("Invalid_Token", "Authenticated user identifier is missing.")]);
        }

        var result = await identityService.SoftDeleteAsync(userId, cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Response.Headers[ForceLogoutHeaderName] = "true";
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }

    [HttpDelete("{userId}")]
    [Authorize(Policy = AuthorizationPolicies.UsersWrite)]
    [ProducesResponseType(typeof(Result<Deleted>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<object?>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser([FromRoute] string userId, CancellationToken cancellationToken)
    {
        var result = await identityService.SoftDeleteAsync(userId, cancellationToken);

        return result.Match(
            onValue: deleted =>
            {
                Result<Deleted> response = deleted;
                return Ok(response);
            },
            onError: Problem);
    }
}
