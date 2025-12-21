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
     // Get all users
   var usersQuery = _userManager.Users.AsQueryable();

        // Apply search filter
     if (!string.IsNullOrWhiteSpace(search))
  {
       search = search.ToLower();
       usersQuery = usersQuery.Where(s =>
         (s.Email != null && s.Email.ToLower().Contains(search)) ||
        (s.UserName != null && s.UserName.ToLower().Contains(search)));
       }

      var totalUsers = await usersQuery.CountAsync();

   // Apply pagination
    var users = await usersQuery
       .OrderBy(s => s.UserName)
  .Skip((page - 1) * pageSize)
                .Take(pageSize)
      .ToListAsync();

            // Filter to only students and get additional info
            var studentDetails = new List<object>();
   foreach (var user in users)
            {
   // Check if user is a student
          if (!await _userManager.IsInRoleAsync(user, "Student"))
          continue;

     var documentCount = await _db.Documents
   .CountAsync(d => d.OwnerUserId == user.Id);

 var pendingSubmissions = await _db.Submissions
      .CountAsync(s => s.StudentId == user.Id && s.Status == "Pending");

     // Get advisor info
           object? advisorInfo = null;
        if (user.AdvisorId != null)
     {
         var advisor = await _userManager.FindByIdAsync(user.AdvisorId);
     if (advisor != null)
        {
    advisorInfo = new
        {
           id = advisor.Id,
         userName = advisor.UserName,
          email = advisor.Email
        };
   }
         }

       studentDetails.Add(new
              {
     id = user.Id,
               userName = user.UserName,
       email = user.Email,
          emailConfirmed = user.EmailConfirmed,
          documentCount,
 pendingSubmissions,
       hasAdvisor = user.AdvisorId != null,
         advisor = advisorInfo
      });
            }

return Ok(new
        {
                totalCount = studentDetails.Count,
     page,
pageSize,
   totalPages = (int)Math.Ceiling(studentDetails.Count / (double)pageSize),
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

        // Öðretmen bilgisi (YENÝ)
        object? advisorInfo = null;
        if (student.AdvisorId != null)
 {
    var advisor = await _userManager.FindByIdAsync(student.AdvisorId);
   if (advisor != null)
 {
           advisorInfo = new
   {
   id = advisor.Id,
  userName = advisor.UserName,
email = advisor.Email
         };
 }
  }

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
    advisor = advisorInfo,
      hasAdvisor = student.AdvisorId != null,
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
   try
 {
       var students = await _userManager.Users
        .Where(u => u.AdvisorId == null)
     .ToListAsync();

      // Filter to only students
 var studentList = new List<object>();
    foreach (var user in students)
      {
     if (await _userManager.IsInRoleAsync(user, "Student"))
    {
     var documentCount = await _db.Documents
      .CountAsync(d => d.OwnerUserId == user.Id);

      studentList.Add(new
          {
          id = user.Id,
       userName = user.UserName,
 email = user.Email,
           emailConfirmed = user.EmailConfirmed,
       documentCount
        });
  }
 }

      return Ok(new
      {
     totalCount = studentList.Count,
       students = studentList
 });
   }
        catch (Exception ex)
      {
    return StatusCode(500, new { error = "Failed to retrieve students without advisor", details = ex.Message });
        }
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

    // Advisor: Get my students
    [HttpGet("my-students")]
    [Authorize(Roles = "Advisor")]
    public async Task<IActionResult> GetMyStudents()
    {
      try
        {
            var advisorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(advisorId))
       return Unauthorized(new { error = "User ID not found in token" });

          // Get students assigned to this advisor
       var students = await _userManager.Users
 .Where(u => u.AdvisorId == advisorId)
        .ToListAsync();

         // Filter to only students (extra safety)
            var studentDetails = new List<object>();
          foreach (var student in students)
      {
      if (!await _userManager.IsInRoleAsync(student, "Student"))
   continue;

    var documentCount = await _db.Documents
   .CountAsync(d => d.OwnerUserId == student.Id);

             var pendingSubmissions = await _db.Submissions
   .CountAsync(s => s.StudentId == student.Id && s.Status == "Pending");

     studentDetails.Add(new
    {
           id = student.Id,
     userName = student.UserName,
    email = student.Email,
     emailConfirmed = student.EmailConfirmed,
      documentCount,
         pendingSubmissions
       });
            }

         return Ok(new
 {
          totalStudents = studentDetails.Count,
     students = studentDetails
            });
        }
        catch (Exception ex)
        {
       return StatusCode(500, new { error = "Failed to retrieve students", details = ex.Message });
        }
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
