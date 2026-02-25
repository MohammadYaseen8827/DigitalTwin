using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AntiForgeryController(IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>
    /// Gets CSRF protection tokens for SPA applications
    /// </summary>
    /// <returns>CSRF token pair for header and form usage</returns>
    [HttpGet("tokens")]
    [ProducesResponseType(typeof(AntiForgeryTokenResponse), StatusCodes.Status200OK)]
    public IActionResult GetTokens()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        
        return Ok(new AntiForgeryTokenResponse
        {
            HeaderName = tokens.HeaderName ?? "RequestVerificationToken",
            RequestToken = tokens.RequestToken ?? string.Empty,
            FormFieldName = tokens.FormFieldName ?? "RequestVerificationToken"
        });
    }

    /// <summary>
    /// Validates a CSRF token (for testing purposes)
    /// </summary>
    /// <param name="request">The validation request containing the token</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
            return Ok(new { valid = true });
        }
        catch (AntiforgeryValidationException)
        {
            return BadRequest(new { valid = false, error = "Invalid CSRF token" });
        }
    }
}

public class AntiForgeryTokenResponse
{
    public string HeaderName { get; set; } = string.Empty;
    public string RequestToken { get; set; } = string.Empty;
    public string FormFieldName { get; set; } = string.Empty;
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}