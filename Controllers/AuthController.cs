using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<AppUser> userManager,
                          SignInManager<AppUser> signInManager,
                          IConfiguration config,
                          ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _logger = logger;
    }

    public record RegisterDto(string Email, string Password, string? FullName);
    public record LoginDto(string Email, string Password);

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new AppUser { UserName = dto.Email, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Register failed for {Email}: {Errors}", dto.Email, string.Join(';', result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }

        // Varsayılan rol olarak Student ata
        var roleResult = await _userManager.AddToRoleAsync(user, "Student");
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign Student role to {Email}: {Errors}", 
                                dto.Email, string.Join(';', roleResult.Errors.Select(e => e.Description)));
            // Role atanamadı ama kullanıcı oluşturuldu, uyarı döndür
            return Ok(new { 
                message = "User created but role assignment failed. Please contact administrator.",
                userId = user.Id,
                warning = "Student role not assigned"
            });
        }

        _logger.LogInformation("User {Email} registered successfully with Student role", dto.Email);
        return Ok(new { message = "Registration successful", userId = user.Id });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found ({Email})", dto.Email);
            return Unauthorized();
        }

        var pass = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!pass.Succeeded)
        {
            _logger.LogWarning("Login failed for {Email}: invalid password", dto.Email);
            return Unauthorized();
        }

        var token = await GenerateTokenAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresMinutes"]!));
        
        return Ok(new { 
       token = token,
 expiresAt = expiresAt,
            expiresIn = int.Parse(_config["Jwt:ExpiresMinutes"]!) * 60 // seconds
        });
    }

    // Yeni: Token yenileme endpoint'i
    [HttpPost("refresh")]
    [Authorize]
    [EnableRateLimiting("auth-relaxed")]
    public async Task<IActionResult> RefreshToken()
    {
  try
        {
    // Mevcut kullanıcıyı token'dan al
  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
   ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
         ?? User.FindFirstValue("sub");

  if (string.IsNullOrEmpty(userId))
      {
         _logger.LogWarning("Refresh token failed: user ID not found in claims");
   return Unauthorized(new { error = "Invalid token" });
            }

            var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
 {
   _logger.LogWarning("Refresh token failed: user {UserId} not found", userId);
              return Unauthorized(new { error = "User not found" });
     }

     // Yeni token oluştur
            var newToken = await GenerateTokenAsync(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresMinutes"]!));

    _logger.LogInformation("Token refreshed for user {UserId}", userId);

  return Ok(new { 
          token = newToken,
        expiresAt = expiresAt,
  expiresIn = int.Parse(_config["Jwt:ExpiresMinutes"]!) * 60
      });
      }
    catch (Exception ex)
        {
         _logger.LogError(ex, "Failed to refresh token");
   return StatusCode(500, new { error = "Failed to refresh token" });
        }
    }

// Yeni: Token validation endpoint
    [HttpGet("validate")]
    [Authorize]
  [EnableRateLimiting("auth-relaxed")]
    public async Task<IActionResult> ValidateToken()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

   if (string.IsNullOrEmpty(userId))
{
      return Ok(new { valid = false, message = "User ID not found" });
   }

            var user = await _userManager.FindByIdAsync(userId);
   if (user == null)
 {
      return Ok(new { valid = false, message = "User not found" });
}

 var roles = await _userManager.GetRolesAsync(user);

 return Ok(new { 
          valid = true,
   userId = user.Id,
                email = user.Email,
          roles = roles
    });
        }
        catch
        {
 return Ok(new { valid = false, message = "Token validation failed" });
        }
    }

    private async Task<string> GenerateTokenAsync(AppUser user)
    {
    var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var roles = await _userManager.GetRolesAsync(user);
        
  // Add multiple claim types for better compatibility
        var claims = new List<Claim>
        {
      // Standard JWT claims
 new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
  new(JwtRegisteredClaimNames.Name, user.UserName ?? ""),
   new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
         
      // Additional claims for compatibility
         new(ClaimTypes.NameIdentifier, user.Id),
 new(ClaimTypes.Name, user.UserName ?? ""),
       new(ClaimTypes.Email, user.Email ?? ""),
    
            // Custom claim
            new("uid", user.Id)
 };
      
        // Add roles
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!));

        var token = new JwtSecurityToken(
      issuer: jwt["Issuer"],
            audience: jwt["Audience"],
      claims: claims,
   expires: expires,
          signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("Token generated for user {UserId} with {RoleCount} roles", user.Id, roles.Count);
        
        return tokenString;
    }
}
