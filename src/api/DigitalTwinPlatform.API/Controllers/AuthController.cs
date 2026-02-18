using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using DigitalTwinPlatform.Domain.Entities.Auth;
using DigitalTwinPlatform.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for user authentication and authorization.
/// Handles user registration, login, and token management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiVersion("1.0")]
[Authorize]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    RefreshTokenService refreshTokenService,
    ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Registers a new user in the Digital Twin Platform.
    /// </summary>
    /// <param name="request">The registration request containing email, password, and full name.</param>
    /// <returns>Success message on successful registration.</returns>
    /// <example>
    /// {
    ///   "email": "operator@example.com",
    ///   "password": "SecurePassword123",
    ///   "fullName": "John Operator",
    ///   "confirmPassword": "SecurePassword123",
    ///   "acceptTerms": true
    /// }
    /// </example>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Invalid registration data or user already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("register")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validate confirm password if provided
        if (!string.IsNullOrEmpty(request.ConfirmPassword) && request.Password != request.ConfirmPassword)
        {
            return BadRequest(new RegisterErrorResponse
            {
                Message = "Registration failed",
                Errors = new List<string> { "Password and confirmation password do not match." }
            });
        }

        // Validate terms acceptance if provided
        if (request.AcceptTerms.HasValue && !request.AcceptTerms.Value)
        {
            return BadRequest(new RegisterErrorResponse
            {
                Message = "Registration failed",
                Errors = new List<string> { "You must accept the terms and conditions to register." }
            });
        }

        var fullName = request.FullName ?? request.Name ?? request.Email;
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = fullName
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new RegisterErrorResponse
            {
                Message = "Registration failed",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        return Ok(new RegisterResponse
        {
            Message = "User registered successfully",
            UserId = user.Id,
            Email = user.Email
        });
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>JWT access token, refresh token, and user information.</returns>
    /// <example>
    /// {
    ///   "email": "operator@example.com",
    ///   "password": "SecurePassword123"
    /// }
    /// </example>
    /// <response code="200">Login successful, returns tokens.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new UnauthorizedResponse
            {
                Message = "Invalid credentials",
                Detail = "The email or password provided is incorrect."
            });
        }

        var tokenResponse = await refreshTokenService.GenerateTokensAsync(user.Id);
        
        if (tokenResponse == null)
        {
            return StatusCode(500, new ErrorResponse
            {
                Message = "Token generation failed",
                Detail = "Unable to generate authentication tokens."
            });
        }

        return Ok(new TokenResponse
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer",
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName
            }
        });
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token request containing the refresh token.</param>
    /// <returns>New access and refresh tokens.</returns>
    /// <example>
    /// {
    ///   "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// </example>
    /// <response code="200">Tokens refreshed successfully.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await refreshTokenService.RefreshAccessTokenAsync(request.RefreshToken);
        
        if (result == null)
        {
            return Unauthorized(new UnauthorizedResponse
            {
                Message = "Invalid refresh token",
                Detail = "The provided refresh token is invalid or has expired."
            });
        }

        return Ok(new TokenResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }

    /// <summary>
    /// Revokes a refresh token, logging the user out.
    /// </summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <returns>Success message on successful revocation.</returns>
    /// <response code="200">Token revoked successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("revoke")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
    {
        await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return Ok(new { Message = "Token revoked successfully" });
    }

    /// <summary>
    /// Gets the current user's profile information.
    /// </summary>
    /// <returns>The current user's profile details.</returns>
    /// <response code="200">Returns user profile successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User not authenticated" });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new CurrentUserResponse
        {
            Id = user.Id,
            Name = user.FullName,
            Email = user.Email!,
            Role = roles.FirstOrDefault() ?? "user",
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">The profile update request.</param>
    /// <returns>Updated user profile.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpPut("profile")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User not authenticated" });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            user.FullName = request.Name;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { Message = "Failed to update profile", Errors = result.Errors.Select(e => e.Description) });
        }

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new CurrentUserResponse
        {
            Id = user.Id,
            Name = user.FullName,
            Email = user.Email!,
            Role = roles.FirstOrDefault() ?? "user",
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    /// <param name="request">The password change request.</param>
    /// <returns>Success message on successful password change.</returns>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Invalid request or current password.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpPost("change-password")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User not authenticated" });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new { Message = "Failed to change password", Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "Password changed successfully" });
    }

    /// <summary>
    /// Initiates password reset process.
    /// </summary>
    /// <param name="request">The forgot password request.</param>
    /// <returns>Success message.</returns>
    [HttpPost("forgot-password")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if user exists
            return Ok(new { Message = "If an account exists with this email, a password reset link has been sent." });
        }

        // Generate password reset token
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        
        // In development/simulation, we log the token since we don't have an SMTP server
        logger.LogInformation("Password reset requested for {Email}. Token: {Token}", request.Email, token);

        // In production, send email with reset link
        _ = Task.Run(async () => {
             try 
             {
                 // SMTP logic would go here
                 // await _emailService.SendPasswordResetAsync(request.Email, token);
                 await Task.CompletedTask;
             }
             catch (Exception ex)
             {
                 logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
             }
        });

        return Ok(new { Message = "If an account exists with this email, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets password using reset token.
    /// </summary>
    /// <param name="request">The reset password request.</param>
    /// <returns>Success message.</returns>
    [HttpPost("reset-password")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest(new { Message = "Invalid password reset request" });
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { Message = "Failed to reset password", Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "Password has been reset successfully" });
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <returns>Success message.</returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await refreshTokenService.RevokeAllUserTokensAsync(userId);
        }

        return Ok(new { Message = "Logged out successfully" });
    }

    /// <summary>
    /// Enables two-factor authentication for the current user.
    /// </summary>
    /// <returns>QR code URI for setting up authenticator app.</returns>
    /// <response code="200">Returns QR code for 2FA setup.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpPost("2fa/enable")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User not authenticated" });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        // Generate 2FA secret and QR code
        var unformattedKey = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 1);
        var token = await userManager.GenerateTwoFactorTokenAsync(user, "totp");
        
        // In a real implementation, we would generate a proper QR code
        // For now, return a placeholder that can be used by the frontend
        var userName = user.Email;
        var issuer = "DigitalTwinPlatform";
        var setupInfo = $"otpauth://totp/{issuer}:{userName}?secret={token}&issuer={issuer}";

        return Ok(new { qrCode = setupInfo, secret = token });
    }

    /// <summary>
    /// Verifies two-factor authentication code and enables 2FA for the user.
    /// </summary>
    /// <param name="request">The verification request containing the code.</param>
    /// <returns>Success message if verification succeeds.</returns>
    /// <response code="200">2FA verified and enabled successfully.</response>
    /// <response code="400">Invalid verification code.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpPost("2fa/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User not authenticated" });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(user, "totp", request.Code);
        if (!isValid)
        {
            return BadRequest(new { Message = "Invalid verification code" });
        }

        // Enable 2FA for the user
        user.TwoFactorEnabled = true;
        var result = await userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { Message = "Failed to enable 2FA", Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "2FA enabled successfully" });
    }

    /// <summary>
    /// Disables two-factor authentication for the current user.
    /// </summary>
    /// <param name="request">The disable request containing the code for verification.</param>
    /// <returns>Success message if 2FA is disabled.</returns>
    /// <response code="200">2FA disabled successfully.</response>
    /// <response code="400">Invalid verification code.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpPost("2fa/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DisableTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User not authenticated" });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        // For disabling 2FA, we might not need code verification in some implementations
        // But for security, we'll require the 2FA code
        if (user.TwoFactorEnabled)
        {
            var isValid = await userManager.VerifyTwoFactorTokenAsync(user, "totp", request.Code);
            if (!isValid)
            {
                return BadRequest(new { Message = "Invalid verification code" });
            }
        }

        // Disable 2FA for the user
        user.TwoFactorEnabled = false;
        var result = await userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { Message = "Failed to disable 2FA", Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "2FA disabled successfully" });
    }
}

/// <summary>
/// Request model for 2FA verification.
/// </summary>
public class VerifyTwoFactorRequest
{
    /// <summary>
    /// The 2FA verification code.
    /// </summary>
    public string Code { get; set; } = null!;
}

/// <summary>
/// Request model for user registration.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's password (minimum 6 characters).
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// User's full name. Frontend may send as "name"; both Name and FullName are accepted.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// User's display name (alternative to FullName for API contract alignment with frontend).
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Confirmation of the user's password (for frontend validation).
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("confirmPassword")]
    public string? ConfirmPassword { get; set; }

    /// <summary>
    /// Whether the user accepts the terms and conditions (for frontend validation).
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("acceptTerms")]
    public bool? AcceptTerms { get; set; }
}

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// User's password.
    /// </summary>
    public string Password { get; set; } = null!;
}

/// <summary>
/// Request model for token refresh.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token string.
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}

/// <summary>
/// Response model for successful registration.
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = null!;
    
    /// <summary>
    /// The created user's ID.
    /// </summary>
    public string UserId { get; set; } = null!;
    
    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = null!;
}

/// <summary>
/// Response model for registration errors.
/// </summary>
public class RegisterErrorResponse
{
    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = null!;
    
    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Response model for successful authentication.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = null!;
    
    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = null!;
    
    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Token type (always "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// User information (included in login response).
    /// </summary>
    public UserInfo? User { get; set; }
}

/// <summary>
/// User information model.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's full name. Serialized as "name" for API contract alignment with frontend.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string FullName { get; set; } = null!;
}

/// <summary>
/// Response model for unauthorized access.
/// </summary>
public class UnauthorizedResponse
{
    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = null!;
    
    /// <summary>
    /// Detailed error description.
    /// </summary>
    public string? Detail { get; set; }
}

/// <summary>
/// Generic error response model.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = null!;
    
    /// <summary>
    /// Detailed error description.
    /// </summary>
    public string? Detail { get; set; }
}

/// <summary>
/// Response model for current user profile.
/// </summary>
public class CurrentUserResponse
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public string Id { get; set; } = null!;
    
    /// <summary>
    /// User's full name.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// User's role.
    /// </summary>
    public string Role { get; set; } = "user";
    
    /// <summary>
    /// Whether 2FA is enabled.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }
    
    /// <summary>
    /// Last login timestamp.
    /// </summary>
    public DateTime? LastLogin { get; set; }
    
    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for profile update.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// User's full name.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Request model for password change.
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Current password.
    /// </summary>
    public string CurrentPassword { get; set; } = null!;
    
    /// <summary>
    /// New password.
    /// </summary>
    public string NewPassword { get; set; } = null!;
}

/// <summary>
/// Request model for forgot password.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;
}

/// <summary>
/// Request model for password reset.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Reset token.
    /// </summary>
    public string Token { get; set; } = null!;
    
    /// <summary>
    /// New password.
    /// </summary>
    public string Password { get; set; } = null!;
}
