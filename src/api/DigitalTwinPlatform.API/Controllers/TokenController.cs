using DigitalTwinPlatform.API.Services.Auth;
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
        // In a real implementation, you would invalidate the refresh token
        // This is a simplified version
        return Ok(new { message = "Token revoked successfully" });
    }
}

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}