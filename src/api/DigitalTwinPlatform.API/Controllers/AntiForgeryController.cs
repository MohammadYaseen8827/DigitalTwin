using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
            HeaderName = tokens.HeaderName,
            RequestToken = tokens.RequestToken,
            FormFieldName = tokens.FormFieldName
        });
    }

    /// <summary>
    /// Validates a CSRF token (for testing purposes)
    /// </summary>
    /// <param name="token">The token to validate</param>
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