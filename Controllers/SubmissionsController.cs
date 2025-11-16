using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _users;

    public SubmissionsController(AppDbContext db, UserManager<AppUser> users)
    {
 _db = db;
_users = users;
    }

    private string? GetUserId()
    {
        try
        {
          var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub")
    ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
      ?? User.Identity?.Name;

        return userId;
        }
        catch
        {
 return null;
        }
    }

    // Get all submissions or my submissions based on role
    [HttpGet("my")]
    public async Task<IActionResult> GetMySubmissions()
    {
     try
        {
      var uid = GetUserId();
        if (string.IsNullOrEmpty(uid))
      return Unauthorized(new { error = "User identification failed" });

            // Check if user is Admin or Advisor
            var isAdmin = User.IsInRole("Admin");
            var isAdvisor = User.IsInRole("Advisor");

        List<Submission> submissions;

            if (isAdmin || isAdvisor)
    {
    // Admin/Advisor can see all submissions
                submissions = await _db.Submissions
          .OrderBy(s => s.DueDate)
          .ToListAsync();
      }
        else
        {
    // Students see only their submissions
      submissions = await _db.Submissions
    .Where(s => s.StudentId == uid)
          .OrderBy(s => s.DueDate)
         .ToListAsync();
            }

   return Ok(submissions);
        }
      catch (Exception ex)
        {
      return StatusCode(500, new { error = "Failed to retrieve submissions", details = ex.Message });
        }
    }

    // Yeni teslim tarihi oluþtur
    [HttpPost]
    [Authorize(Roles = "Advisor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto)
    {
        var submission = new Submission
    {
            StudentId = dto.StudentId,
       DueDate = dto.DueDate,
  Status = "Pending"
        };
 _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();
        return Ok(new { submission.Id });
    }

    // Teslim durumunu güncelle
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var uid = GetUserId();
      var submission = await _db.Submissions.FindAsync(id);
        if (submission == null) return NotFound();
      if (submission.StudentId != uid) return Forbid();

      submission.Status = dto.Status;
        await _db.SaveChangesAsync();
        return Ok(new { submission.Status });
    }

    public record CreateSubmissionDto(string StudentId, DateTime DueDate);
    public record UpdateStatusDto(string Status);
}
