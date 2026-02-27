using DigitalTwinPlatform.API.Services.Auth;
using DigitalTwinPlatform.Application.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController(RefreshTokenService refreshTokenService) : ControllerBase
{
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access and refresh tokens</returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required" });
        }

        var result = await refreshTokenService.RefreshAccessTokenAsync(request.RefreshToken);
        
        if (result == null)
        {
            return Unauthorized(new { error = "Invalid or expired refresh token" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Revoke refresh token (logout)
    /// </summary>
    /// <param name="request">Revoke token request</param>
    /// <returns>Success response</returns>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required" });
        }

        await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return Ok(new { message = "Token revoked successfully" });
    }
}