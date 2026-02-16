using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace Auth.API.Controllers;

/// <summary>
/// Authentication Controller
/// Handles login, register, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ITokenService tokenService,
        IConnectionMultiplexer redis,
        ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// User login - generates JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        try
        {
            var db = _redis.GetDatabase();

            // Get user from Redis (in real scenario, from PostgreSQL)
            var userKey = $"user:email:{request.Email}";
            var userJson = await db.StringGetAsync(userKey);

            if (userJson.IsNullOrEmpty)
            {
                _logger.LogWarning("Login failed: User not found - {Email}", request.Email);
                return Unauthorized(new LoginResponse(
                    false,
                    null,
                    null,
                    null,
                    "Invalid email or password"));
            }

            var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson!);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new LoginResponse(
                    false,
                    null,
                    null,
                    null,
                    "Invalid email or password"));
            }

            // Verify password
            var passwordHash = HashPassword(request.Password);
            if (user.PasswordHash != passwordHash)
            {
                _logger.LogWarning("Login failed: Invalid password - {Email}", request.Email);
                return Unauthorized(new LoginResponse(
                    false,
                    null,
                    null,
                    null,
                    "Invalid email or password"));
            }

            // Generate JWT token
            var token = _tokenService.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            // Store refresh token in Redis
            var refreshTokenKey = $"refreshtoken:{user.Id}";
            await db.StringSetAsync(refreshTokenKey, refreshToken, TimeSpan.FromDays(7));

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await db.StringSetAsync(userKey, System.Text.Json.JsonSerializer.Serialize(user));

            _logger.LogInformation("Login successful for user: {UserId}", user.Id);

            return Ok(new LoginResponse(
                true,
                token,
                refreshToken,
                expiresAt,
                "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new LoginResponse(
                false,
                null,
                null,
                null,
                "An error occurred during login"));
        }
    }

    /// <summary>
    /// User registration
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        try
        {
            var db = _redis.GetDatabase();

            // Check if user already exists
            var userKey = $"user:email:{request.Email}";
            var existingUser = await db.StringGetAsync(userKey);

            if (!existingUser.IsNullOrEmpty)
            {
                return BadRequest(new RegisterResponse(
                    false,
                    null,
                    "User with this email already exists"));
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Store user in Redis
            var userJson = System.Text.Json.JsonSerializer.Serialize(user);
            await db.StringSetAsync(userKey, userJson);
            await db.StringSetAsync($"user:id:{user.Id}", userJson);

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);

            return CreatedAtAction(
                nameof(Login),
                new RegisterResponse(
                    true,
                    user.Id,
                    "Registration successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new RegisterResponse(
                false,
                null,
                "An error occurred during registration"));
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var db = _redis.GetDatabase();

            // Validate refresh token
            var refreshTokenKey = $"refreshtoken:{request.UserId}";
            var storedRefreshToken = await db.StringGetAsync(refreshTokenKey);

            if (storedRefreshToken.IsNullOrEmpty || storedRefreshToken != request.RefreshToken)
            {
                return Unauthorized(new LoginResponse(
                    false,
                    null,
                    null,
                    null,
                    "Invalid refresh token"));
            }

            // Get user
            var userKey = $"user:id:{request.UserId}";
            var userJson = await db.StringGetAsync(userKey);

            if (userJson.IsNullOrEmpty)
            {
                return Unauthorized(new LoginResponse(
                    false,
                    null,
                    null,
                    null,
                    "User not found"));
            }

            var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson!);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new LoginResponse(
                    false,
                    null,
                    null,
                    null,
                    "User is not active"));
            }

            // Generate new tokens
            var newToken = _tokenService.GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            // Update refresh token in Redis
            await db.StringSetAsync(refreshTokenKey, newRefreshToken, TimeSpan.FromDays(7));

            return Ok(new LoginResponse(
                true,
                newToken,
                newRefreshToken,
                expiresAt,
                "Token refreshed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new LoginResponse(
                false,
                null,
                null,
                null,
                "An error occurred while refreshing token"));
        }
    }

    /// <summary>
    /// Logout - invalidate tokens
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            var db = _redis.GetDatabase();
            var refreshTokenKey = $"refreshtoken:{request.UserId}";
            
            await db.KeyDeleteAsync(refreshTokenKey);

            _logger.LogInformation("User logged out: {UserId}", request.UserId);

            return Ok(new { success = true, message = "Logout successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { success = false, message = "An error occurred during logout" });
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

public record RefreshTokenRequest(Guid UserId, string RefreshToken);
public record LogoutRequest(Guid UserId);
