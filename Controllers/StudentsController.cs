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

    [HttpGet]
    public async Task<IActionResult> GetAllStudents(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
 try
  {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
   var isAdmin = User.IsInRole("Admin");
            var isAdvisor = User.IsInRole("Advisor");

            var usersQuery = _userManager.Users.AsQueryable();

if (isAdvisor && !isAdmin)
       {
      usersQuery = usersQuery.Where(u => u.AdvisorId == userId);
      }

  if (!string.IsNullOrWhiteSpace(search))
         {
       search = search.ToLower();
    usersQuery = usersQuery.Where(s =>
    (s.Email != null && s.Email.ToLower().Contains(search)) ||
    (s.UserName != null && s.UserName.ToLower().Contains(search)));
    }

         var totalUsers = await usersQuery.CountAsync();

            var users = await usersQuery
      .OrderBy(s => s.UserName)
   .Skip((page - 1) * pageSize)
   .Take(pageSize)
       .ToListAsync();

      var studentDetails = new List<object>();
            foreach (var user in users)
            {
           if (!await _userManager.IsInRoleAsync(user, "Student"))
          continue;

                var documentCount = await _db.Documents
            .CountAsync(d => d.OwnerUserId == user.Id);

      var pendingSubmissions = await _db.Submissions
              .CountAsync(s => s.StudentId == user.Id && s.Status == "Pending");

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(string id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        var isAdvisor = User.IsInRole("Advisor");

        var student = await _userManager.FindByIdAsync(id);
        if (student == null)
     return NotFound(new { error = "Student not found" });

        if (!await _userManager.IsInRoleAsync(student, "Student"))
            return BadRequest(new { error = "User is not a student" });

      if (isAdvisor && !isAdmin && student.AdvisorId != userId)
            return Forbid();

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

      var submissions = await _db.Submissions
    .Where(s => s.StudentId == id)
    .OrderByDescending(s => s.DueDate)
            .ToListAsync();

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
documents,
 submissions,
    unreadNotifications
      });
    }

    [HttpPost("{id}/send-notification")]
    public async Task<IActionResult> SendNotificationToStudent(string id, [FromBody] SendNotificationDto dto)
    {
   try
        {
       var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
 var isAdvisor = User.IsInRole("Advisor");

   var student = await _userManager.FindByIdAsync(id);
            if (student == null)
      return NotFound(new { error = "Student not found" });

            if (!await _userManager.IsInRoleAsync(student, "Student"))
           return BadRequest(new { error = "User is not a student" });

      if (isAdvisor && !isAdmin && student.AdvisorId != userId)
            return Forbid();

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
   return StatusCode(500, new
         {
      error = "Failed to send notification",
 details = ex.Message,
innerError = ex.InnerException?.Message
      });
        }
    }

    [HttpPost("send-bulk-notification")]
    public async Task<IActionResult> SendBulkNotification([FromBody] BulkNotificationDto dto)
    {
        if (dto.StudentIds == null || !dto.StudentIds.Any())
      return BadRequest("At least one student ID is required");

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        var isAdvisor = User.IsInRole("Advisor");

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

 if (isAdvisor && !isAdmin && student.AdvisorId != userId)
          {
    failedCount++;
        errors.Add($"Student {studentId} is not assigned to you");
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
    successCount,
      failedCount,
        errors
      });
    }

  [HttpPost("send-notification-to-all")]
  [Authorize(Roles = "Admin")]
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
          }
        }

        return Ok(new
        {
          message = $"Notification sent to {successCount} students",
          totalStudents = students.Count,
   successCount
        });
    }

    [HttpGet("without-advisor")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStudentsWithoutAdvisor()
    {
        try
        {
    var students = await _userManager.Users
       .Where(u => u.AdvisorId == null)
     .ToListAsync();

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

    [HttpGet("with-pending-submissions")]
    public async Task<IActionResult> GetStudentsWithPendingSubmissions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        var isAdvisor = User.IsInRole("Advisor");

        var submissionsQuery = _db.Submissions.Where(s => s.Status == "Pending");

        if (isAdvisor && !isAdmin)
        {
  var myStudentIds = await _userManager.Users
            .Where(u => u.AdvisorId == userId)
       .Select(u => u.Id)
   .ToListAsync();

        submissionsQuery = submissionsQuery.Where(s => myStudentIds.Contains(s.StudentId));
        }

        var studentsWithPending = await submissionsQuery
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

    [HttpGet("my-students")]
    [Authorize(Roles = "Advisor")]
    public async Task<IActionResult> GetMyStudents()
    {
        try
        {
  var advisorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(advisorId))
            return Unauthorized(new { error = "User ID not found in token" });

    var students = await _userManager.Users
     .Where(u => u.AdvisorId == advisorId)
           .ToListAsync();

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
