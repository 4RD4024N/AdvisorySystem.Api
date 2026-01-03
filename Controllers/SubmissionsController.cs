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
return User.FindFirstValue(ClaimTypes.NameIdentifier)
   ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
           ?? User.Identity?.Name;
        }
   catch
     {
       return null;
     }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMySubmissions()
{
        try
   {
            var uid = GetUserId();
            if (string.IsNullOrEmpty(uid))
     return Unauthorized(new { error = "User identification failed" });

         var isAdmin = User.IsInRole("Admin");
   var isAdvisor = User.IsInRole("Advisor");

  List<Submission> submissions;

    if (isAdmin)
     {
         submissions = await _db.Submissions
         .OrderBy(s => s.DueDate)
           .ToListAsync();
  }
   else if (isAdvisor)
   {
           var myStudentIds = await _users.Users
    .Where(u => u.AdvisorId == uid)
            .Select(u => u.Id)
              .ToListAsync();

       submissions = await _db.Submissions
      .Where(s => myStudentIds.Contains(s.StudentId))
             .OrderBy(s => s.DueDate)
        .ToListAsync();
  }
      else
            {
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

    // ? ADMIN SUBMISSION OLUÞTURAMAZ - Sadece Advisor
    [HttpPost]
    [Authorize(Roles = "Advisor")]
    public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto)
    {
      try
        {
      var uid = GetUserId();
if (string.IsNullOrEmpty(uid))
  return Unauthorized();

       AppUser? student = null;

            if (!string.IsNullOrEmpty(dto.StudentId))
 {
student = await _users.FindByIdAsync(dto.StudentId);
   }
   else if (!string.IsNullOrEmpty(dto.StudentEmail))
    {
          student = await _users.FindByEmailAsync(dto.StudentEmail);
       }

   if (student == null)
return NotFound(new { error = "Student not found. Please provide valid student ID or email." });

    if (!await _users.IsInRoleAsync(student, "Student"))
  {
     return BadRequest(new { error = "User is not a student" });
        }

    // Advisor sadece kendi öðrencilerine atayabilir
       if (student.AdvisorId != uid)
        {
        return Forbid();
     }

     if (dto.DocumentId.HasValue)
    {
       var doc = await _db.Documents.FindAsync(dto.DocumentId.Value);
   if (doc == null)
 return NotFound(new { error = "Document not found" });

           if (doc.OwnerUserId != student.Id)
                {
 return BadRequest(new { error = "Document does not belong to the specified student" });
}
            }

            var submission = new Submission
 {
       StudentId = student.Id,
            DocumentId = dto.DocumentId,
     DueDate = dto.DueDate,
        Status = "Pending",
 CreatedByUserId = uid,
        Notes = dto.Notes
     };

   _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

          await CreateDeadlineNotification(student.Id, submission.Id, dto.DueDate, dto.Notes);

  return Ok(new
        {
   submission.Id,
 studentId = student.Id,
      studentEmail = student.Email,
                message = $"Submission deadline created successfully for {student.Email}"
    });
        }
        catch (Exception ex)
        {
        return StatusCode(500, new { error = "Failed to create submission", details = ex.Message });
        }
    }

    private async Task CreateDeadlineNotification(string studentId, int submissionId, DateTime dueDate, string? notes)
    {
        try
        {
     var message = $"You have a new submission deadline: {dueDate:dd/MM/yyyy HH:mm}";
    if (!string.IsNullOrEmpty(notes))
            {
                message += $"\n\nNotes: {notes}";
        }

 var notification = new Notification
            {
        UserId = studentId,
    Title = "New Submission Deadline",
      Message = message,
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
        }
    }

    public record CreateSubmissionDto(
        string? StudentId,
        string? StudentEmail,
        int? DocumentId,
   DateTime DueDate,
        string? Notes
    );
}
