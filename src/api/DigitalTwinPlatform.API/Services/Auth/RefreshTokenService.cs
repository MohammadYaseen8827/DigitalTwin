using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.API.Services.Auth;

public class RefreshTokenService(
    UserManager<Domain.Entities.Auth.ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<RefreshTokenService> logger)
{
    public async Task<TokenResponse?> GenerateTokensAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("User not found for token generation: {UserId}", userId);
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("FullName", user.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.Parse(configuration["Jwt:ExpiryInMinutes"] ?? "1440");
        
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate refresh token (simplified implementation)
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Store refresh token (in production, use secure storage)
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshTokenExpiry;
        await userManager.UpdateAsync(user);

        logger.LogInformation("Generated new tokens for user {UserId}", userId);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = expiryMinutes * 60,
            User = new UserInfo
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Roles = roles.ToList()
            }
        };
    }

    public async Task<TokenResponse?> RefreshAccessTokenAsync(string refreshToken)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            logger.LogWarning("Invalid or expired refresh token");
            return null;
        }

        return await GenerateTokensAsync(user.Id);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user != null)
        {
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiry = null;
            await userManager.UpdateAsync(user);
            logger.LogInformation("Refresh token revoked for user {UserId}", user.Id);
        }
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiry = null;
            await userManager.UpdateAsync(user);
            logger.LogInformation("All refresh tokens revoked for user {UserId}", userId);
        }
    }

    private string GenerateRefreshToken()
    {
        // In production, use cryptographically secure random generator
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}