using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
 private readonly UserManager<AppUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;

 public DebugController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
 {
 _userManager = userManager;
 _roleManager = roleManager;
 }

 [HttpGet("seedinfo")]
 [AllowAnonymous]
 public async Task<IActionResult> SeedInfo()
 {
 var users = _userManager.Users;
 var userCount = await Task.FromResult(users.Count());
 var roles = _roleManager.Roles;
 var roleCount = await Task.FromResult(roles.Count());

 var firstUser = users.FirstOrDefault();
 return Ok(new
 {
 UserCount = userCount,
 RoleCount = roleCount,
 FirstUser = firstUser == null ? null : new { firstUser.Id, firstUser.UserName, firstUser.Email }
 });
 }

 // Development helper: return a JWT for a given email if the user exists.
 [HttpPost("token/{email}")]
 [AllowAnonymous]
 public async Task<IActionResult> IssueTokenFor(string email, [FromServices] IConfiguration cfg, [FromServices] UserManager<AppUser> userManager)
 {
 var user = await userManager.FindByEmailAsync(email);
 if (user is null) return NotFound();

 var roles = await userManager.GetRolesAsync(user);
 var claims = new List<System.Security.Claims.Claim>
 {
 new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id),
 new(System.Security.Claims.ClaimTypes.Name, user.UserName ?? "")
 };
 claims.AddRange(roles.Select(r => new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r)));

 var jwt = cfg.GetSection("Jwt");
 var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwt["Key"]!));
 var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
 var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!));

 var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
 issuer: jwt["Issuer"],
 audience: jwt["Audience"],
 claims: claims,
 expires: expires,
 signingCredentials: creds);

 var written = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
 return Ok(new { token = written });
 }

 // Get all users with their roles
 [HttpGet("users")]
 [AllowAnonymous]
 public async Task<IActionResult> GetAllUsers()
 {
 var users = await _userManager.Users.ToListAsync();
 var result = new List<object>();
 
 foreach (var user in users)
 {
 var roles = await _userManager.GetRolesAsync(user);
 result.Add(new
 {
 user.Id,
 user.UserName,
 user.Email,
 user.EmailConfirmed,
 Roles = roles
 });
 }
 
 return Ok(result);
 }

 // Delete all users (DANGEROUS - development only)
 [HttpDelete("users/all")]
 [AllowAnonymous]
 public async Task<IActionResult> DeleteAllUsers()
 {
 var users = await _userManager.Users.ToListAsync();
 var deletedCount = 0;
 var errors = new List<string>();

 foreach (var user in users)
 {
 var result = await _userManager.DeleteAsync(user);
 if (result.Succeeded)
 {
 deletedCount++;
 }
 else
 {
 errors.Add($"{user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
 }
 }

 return Ok(new
 {
 DeletedCount = deletedCount,
 TotalUsers = users.Count,
 Errors = errors
 });
 }

 // Yeni: Rolsüz kullanýcýlara Student rolü ata
 [HttpPost("fix-missing-roles")]
 [AllowAnonymous]
 public async Task<IActionResult> FixMissingRoles()
 {
 var users = await _userManager.Users.ToListAsync();
 var fixedCount = 0;
 var alreadyHasRole = 0;
 var errors = new List<string>();

 foreach (var user in users)
 {
 var roles = await _userManager.GetRolesAsync(user);
            
 // Eðer hiç rolü yoksa Student rolü ata
 if (!roles.Any())
 {
var result = await _userManager.AddToRoleAsync(user, "Student");
 if (result.Succeeded)
 {
 fixedCount++;
 }
                else
 {
errors.Add($"{user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
 }
}
      else
 {
         alreadyHasRole++;
 }
 }

        return Ok(new
 {
   message = "Missing roles fixed",
 fixedCount = fixedCount,
    alreadyHadRole = alreadyHasRole,
      totalUsers = users.Count,
   errors = errors
        });
    }

    // Yeni: Rolsüz kullanýcýlarý listele
    [HttpGet("users-without-roles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUsersWithoutRoles()
    {
    var users = await _userManager.Users.ToListAsync();
   var usersWithoutRoles = new List<object>();

        foreach (var user in users)
    {
  var roles = await _userManager.GetRolesAsync(user);
  if (!roles.Any())
            {
        usersWithoutRoles.Add(new
        {
          user.Id,
     user.UserName,
            user.Email,
     user.EmailConfirmed
                });
     }
        }

 return Ok(new
        {
       count = usersWithoutRoles.Count,
         users = usersWithoutRoles
        });
 }
}
