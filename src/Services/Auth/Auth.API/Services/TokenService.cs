using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Services;

/// <summary>
/// JWT Token Service for authentication
/// Generates and validates JWT tokens
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(User user)
    {
        var secret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = _configuration["JWT:Issuer"] ?? "BankingSystem";
        var audience = _configuration["JWT:Audience"] ?? "BankingClients";
        var expirationMinutes = int.Parse(_configuration["JWT:ExpirationInMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("CustomerId", user.CustomerId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("JWT token generated for user {UserId}", user.Id);

        return tokenString;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var secret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var issuer = _configuration["JWT:Issuer"] ?? "BankingSystem";
            var audience = _configuration["JWT:Audience"] ?? "BankingClients";

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }
}

/// <summary>
/// User model for authentication
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Login Request DTO
/// </summary>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Login Response DTO
/// </summary>
public record LoginResponse(
    bool Success,
    string? Token,
    string? RefreshToken,
    DateTime? ExpiresAt,
    string? Message);

/// <summary>
/// Register Request DTO
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    Guid CustomerId);

/// <summary>
/// Register Response DTO
/// </summary>
public record RegisterResponse(
    bool Success,
    Guid? UserId,
    string? Message);
