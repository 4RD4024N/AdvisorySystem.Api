using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Advisor")]
public class StudentsController : ControllerBase
{
  private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public StudentsController(
UserManager<AppUser> userManager, 
        AppDbContext db,
        INotificationService notificationService)
    {
    _userManager = userManager;
        _db = db;
_notificationService = notificationService;
    }

    // Get all students with search and pagination
    [HttpGet]
    public async Task<IActionResult> GetAllStudents(
   [FromQuery] string? search = null,
   [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        try
        {
         // Get all users in Student role
       var students = await _userManager.GetUsersInRoleAsync("Student");

  // Apply search filter
     if (!string.IsNullOrWhiteSpace(search))
    {
  search = search.ToLower();
students = students
     .Where(s => 
      (s.Email != null && s.Email.ToLower().Contains(search)) ||
      (s.UserName != null && s.UserName.ToLower().Contains(search)))
      .ToList();
  }

    var totalCount = students.Count;

  // Apply pagination
      var pagedStudents = students
    .Skip((page - 1) * pageSize)
       .Take(pageSize)
     .ToList();

// Get additional info for each student
            var studentDetails = new List<object>();
     foreach (var student in pagedStudents)
      {
       var documentCount = await _db.Documents
    .CountAsync(d => d.OwnerUserId == student.Id);

          var pendingSubmissions = await _db.Submissions
           .CountAsync(s => s.StudentId == student.Id && s.Status == "Pending");

          var hasAdvisor = await _db.Documents
           .AnyAsync(d => d.OwnerUserId == student.Id && d.AdvisorUserId != null);

    studentDetails.Add(new
 {
         id = student.Id,
    userName = student.UserName,
           email = student.Email,
            emailConfirmed = student.EmailConfirmed,
     documentCount = documentCount,
pendingSubmissions = pendingSubmissions,
      hasAdvisor = hasAdvisor
    });
       }

    return Ok(new
            {
     totalCount = totalCount,
         page = page,
          pageSize = pageSize,
   totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
       students = studentDetails
            });
        }
        catch (Exception ex)
        {
 return StatusCode(500, new { error = "Failed to retrieve students", details = ex.Message });
  }
    }

    // Get student details by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(string id)
    {
        var student = await _userManager.FindByIdAsync(id);
        if (student == null)
      return NotFound("Student not found");

        // Check if user is actually a student
        if (!await _userManager.IsInRoleAsync(student, "Student"))
      return BadRequest("User is not a student");

     // Get student's documents
        var documents = await _db.Documents
    .Where(d => d.OwnerUserId == id)
  .Select(d => new
 {
                d.Id,
        d.Title,
       d.Tags,
        d.CreatedAt,
   versionCount = d.Versions.Count,
       advisorId = d.AdvisorUserId
         })
    .ToListAsync();

  // Get student's submissions
        var submissions = await _db.Submissions
          .Where(s => s.StudentId == id)
      .OrderByDescending(s => s.DueDate)
 .ToListAsync();

        // Get unread notifications count
        var unreadNotifications = await _db.Notifications
            .CountAsync(n => n.UserId == id && !n.IsRead);

        return Ok(new
        {
            id = student.Id,
        userName = student.UserName,
    email = student.Email,
        emailConfirmed = student.EmailConfirmed,
            documents = documents,
      submissions = submissions,
            unreadNotifications = unreadNotifications
});
    }

    // Send notification to single student
    [HttpPost("{id}/send-notification")]
    public async Task<IActionResult> SendNotificationToStudent(string id, [FromBody] SendNotificationDto dto)
    {
        try
        {
            var student = await _userManager.FindByIdAsync(id);
            if (student == null)
           return NotFound(new { error = "Student not found" });

     if (!await _userManager.IsInRoleAsync(student, "Student"))
        return BadRequest(new { error = "User is not a student" });

            await _notificationService.CreateNotificationAsync(
                id,
                dto.Title,
    dto.Message,
            dto.Type,
      dto.RelatedEntityId,
dto.RelatedEntityType
            );

            return Ok(new { message = $"Notification sent to {student.Email}" });
      }
        catch (Exception ex)
        {
return StatusCode(500, new { 
         error = "Failed to send notification", 
      details = ex.Message,
    innerError = ex.InnerException?.Message
  });
        }
    }

    // Send notification to multiple students
    [HttpPost("send-bulk-notification")]
    public async Task<IActionResult> SendBulkNotification([FromBody] BulkNotificationDto dto)
    {
        if (dto.StudentIds == null || !dto.StudentIds.Any())
            return BadRequest("At least one student ID is required");

        var successCount = 0;
        var failedCount = 0;
  var errors = new List<string>();

     foreach (var studentId in dto.StudentIds)
   {
      try
  {
         var student = await _userManager.FindByIdAsync(studentId);
         if (student == null || !await _userManager.IsInRoleAsync(student, "Student"))
      {
     failedCount++;
  errors.Add($"Student {studentId} not found or invalid");
          continue;
     }

             await _notificationService.CreateNotificationAsync(
            studentId,
                dto.Title,
    dto.Message,
      dto.Type,
     dto.RelatedEntityId,
     dto.RelatedEntityType
      );

       successCount++;
            }
catch (Exception ex)
            {
          failedCount++;
                errors.Add($"Failed for student {studentId}: {ex.Message}");
    }
 }

        return Ok(new
        {
            message = $"Notification sent to {successCount} students",
            successCount = successCount,
       failedCount = failedCount,
      errors = errors
        });
    }

    // Send notification to all students
    [HttpPost("send-notification-to-all")]
    public async Task<IActionResult> SendNotificationToAllStudents([FromBody] SendNotificationDto dto)
    {
    var students = await _userManager.GetUsersInRoleAsync("Student");
  var successCount = 0;

        foreach (var student in students)
        {
try
          {
        await _notificationService.CreateNotificationAsync(
          student.Id,
        dto.Title,
     dto.Message,
   dto.Type,
         dto.RelatedEntityId,
             dto.RelatedEntityType
      );
          successCount++;
    }
   catch
 {
       // Log error but continue
          }
        }

        return Ok(new
        {
            message = $"Notification sent to {successCount} students",
    totalStudents = students.Count,
     successCount = successCount
     });
    }

    // Get students without advisor
    [HttpGet("without-advisor")]
    public async Task<IActionResult> GetStudentsWithoutAdvisor()
 {
        var students = await _userManager.GetUsersInRoleAsync("Student");
        var studentsWithoutAdvisor = new List<object>();

      foreach (var student in students)
     {
            var hasAdvisor = await _db.Documents
    .AnyAsync(d => d.OwnerUserId == student.Id && d.AdvisorUserId != null);

        if (!hasAdvisor)
         {
     var documentCount = await _db.Documents
    .CountAsync(d => d.OwnerUserId == student.Id);

       studentsWithoutAdvisor.Add(new
          {
id = student.Id,
   userName = student.UserName,
   email = student.Email,
           documentCount = documentCount
    });
      }
   }

        return Ok(studentsWithoutAdvisor);
    }

    // Get students with pending submissions
 [HttpGet("with-pending-submissions")]
    public async Task<IActionResult> GetStudentsWithPendingSubmissions()
    {
        var studentsWithPending = await _db.Submissions
            .Where(s => s.Status == "Pending")
            .GroupBy(s => s.StudentId)
            .Select(g => new
            {
     studentId = g.Key,
        pendingCount = g.Count(),
  nextDeadline = g.Min(s => s.DueDate)
   })
  .ToListAsync();

        var result = new List<object>();
   foreach (var item in studentsWithPending)
        {
var student = await _userManager.FindByIdAsync(item.studentId);
     if (student != null)
      {
  result.Add(new
             {
          id = student.Id,
     userName = student.UserName,
          email = student.Email,
         pendingSubmissions = item.pendingCount,
nextDeadline = item.nextDeadline
            });
      }
        }

        return Ok(result);
    }

    // DTOs
    public record SendNotificationDto(
 string Title,
        string Message,
        NotificationType Type,
        string? RelatedEntityId = null,
        string? RelatedEntityType = null
    );

    public record BulkNotificationDto(
        List<string> StudentIds,
 string Title,
        string Message,
        NotificationType Type,
        string? RelatedEntityId = null,
        string? RelatedEntityType = null
    );
}
