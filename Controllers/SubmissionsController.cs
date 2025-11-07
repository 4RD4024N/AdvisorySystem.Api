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

    private string GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(sub)) return sub;
        var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
  if (!string.IsNullOrEmpty(nameId)) return nameId;
        return User.Identity?.Name ?? throw new UnauthorizedAccessException("User ID not found");
    }

  // Öðrencinin teslim tarihlerini listele
  [HttpGet("my")]
[Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySubmissions()
    {
        var uid = GetUserId();
        var submissions = await _db.Submissions
            .Where(s => s.StudentId == uid)
            .OrderBy(s => s.DueDate)
        .ToListAsync();
        return Ok(submissions);
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
