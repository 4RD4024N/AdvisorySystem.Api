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

    // Yeni teslim tarihi oluþtur - Danýþman öðrenciye belge için teslim tarihi belirler
    [HttpPost]
    [Authorize(Roles = "Advisor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto)
    {
        try
        {
   var uid = GetUserId();
if (string.IsNullOrEmpty(uid))
     return Unauthorized();

   // Öðrenci var mý kontrol et
     var student = await _users.FindByIdAsync(dto.StudentId);
    if (student == null)
       return NotFound(new { error = "Student not found" });

        // Doküman varsa kontrol et
            if (dto.DocumentId.HasValue)
      {
         var doc = await _db.Documents.FindAsync(dto.DocumentId.Value);
    if (doc == null)
     return NotFound(new { error = "Document not found" });

     // Danýþman sadece kendi öðrencisine teslim atayabilir
  if (User.IsInRole("Advisor") && doc.AdvisorUserId != uid)
    return Forbid();
}

   var submission = new Submission
            {
    StudentId = dto.StudentId,
      DocumentId = dto.DocumentId,
 DueDate = dto.DueDate,
            Status = "Pending",
      CreatedByUserId = uid,
       Notes = dto.Notes
     };
      
      _db.Submissions.Add(submission);
       await _db.SaveChangesAsync();

     // Bildirim oluþtur
  await CreateDeadlineNotification(dto.StudentId, submission.Id, dto.DueDate);

    return Ok(new { 
        submission.Id,
        message = "Submission deadline created successfully"
            });
  }
  catch (Exception ex)
   {
        return StatusCode(500, new { error = "Failed to create submission", details = ex.Message });
    }
    }

 private async Task CreateDeadlineNotification(string studentId, int submissionId, DateTime dueDate)
    {
  try
{
    var notification = new Notification
            {
       UserId = studentId,
   Title = "New Submission Deadline",
        Message = $"You have a new submission deadline: {dueDate:dd/MM/yyyy HH:mm}",
  Type = NotificationType.DeadlineApproaching,
    RelatedEntityId = submissionId.ToString(),
       RelatedEntityType = "Submission",
       IsRead = false
       };

     _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
  }
 catch
        {
    // Bildirim hatasý ana iþlemi etkilemez
 }
    }

    public record CreateSubmissionDto(string StudentId, int? DocumentId, DateTime DueDate, string? Notes);
}
