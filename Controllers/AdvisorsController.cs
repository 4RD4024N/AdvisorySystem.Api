using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("standard")]
public class AdvisorsController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AdvisorsController> _logger;

    public AdvisorsController(
        UserManager<AppUser> userManager,
      INotificationService notificationService,
        ILogger<AdvisorsController> logger)
    {
      _userManager = userManager;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Admin: Get all advisors (teachers)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAdvisors()
    {
    try
        {
 var advisors = await _userManager.GetUsersInRoleAsync("Advisor");
      
            var result = advisors.Select(a => new
        {
          a.Id,
       a.UserName,
       a.Email,
         a.EmailConfirmed
   }).ToList();

   return Ok(new
    {
              totalAdvisors = result.Count,
 advisors = result
     });
  }
        catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to get advisors");
       return StatusCode(500, new { error = "Failed to retrieve advisors" });
        }
  }

    /// <summary>
    /// Admin: Assign advisor to student
    /// </summary>
 [HttpPost("assign")]
    public async Task<IActionResult> AssignAdvisorToStudent([FromBody] AssignAdvisorRequest request)
    {
 try
        {
       // Validate student
  var student = await _userManager.FindByIdAsync(request.StudentId);
            if (student == null)
   return NotFound(new { error = "Student not found" });

            if (!await _userManager.IsInRoleAsync(student, "Student"))
                return BadRequest(new { error = "User is not a student" });

            // Validate advisor
            var advisor = await _userManager.FindByIdAsync(request.AdvisorId);
 if (advisor == null)
    return NotFound(new { error = "Advisor not found" });

 if (!await _userManager.IsInRoleAsync(advisor, "Advisor"))
    return BadRequest(new { error = "User is not an advisor" });

            // Check if already assigned
   var previousAdvisorId = student.AdvisorId;
            var isUpdate = previousAdvisorId != null;

      // Assign advisor
      student.AdvisorId = request.AdvisorId;
          var result = await _userManager.UpdateAsync(student);

     if (!result.Succeeded)
     {
 return StatusCode(500, new
  {
       error = "Failed to assign advisor",
       details = result.Errors.Select(e => e.Description)
      });
      }

            // Send notification to student
          await _notificationService.CreateNotificationAsync(
     student.Id,
        isUpdate ? "Öðretmeniniz Deðiþtirildi" : "Öðretmen Atandý",
      $"{advisor.UserName} öðretmeniniz olarak {(isUpdate ? "güncellendi" : "atandý")}.",
       NotificationType.AdvisorAssigned,
      advisor.Id,
          "Advisor"
  );

          // Send notification to new advisor
            await _notificationService.CreateNotificationAsync(
       advisor.Id,
 "Yeni Öðrenci Atandý",
   $"{student.UserName} öðrenciniz olarak atandý.",
      NotificationType.AdvisorAssigned,
                student.Id,
         "Student"
            );

// If updating, notify previous advisor
        if (isUpdate && previousAdvisorId != null)
    {
    await _notificationService.CreateNotificationAsync(
      previousAdvisorId,
            "Öðrenci Atamasý Kaldýrýldý",
    $"{student.UserName} artýk öðrenciniz deðil.",
  NotificationType.General,
      null,
     null
          );
            }

 _logger.LogInformation(
                "Advisor {AdvisorName} assigned to student {StudentName} by admin",
                advisor.UserName,
    student.UserName
  );

        return Ok(new
    {
         message = isUpdate ? "Öðretmen baþarýyla güncellendi" : "Öðretmen baþarýyla atandý",
         studentId = student.Id,
      studentName = student.UserName,
     advisorId = advisor.Id,
        advisorName = advisor.UserName,
                isUpdate
   });
        }
      catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to assign advisor to student");
  return StatusCode(500, new
        {
      error = "Failed to assign advisor",
     details = ex.Message
            });
        }
    }

    /// <summary>
    /// Admin: Remove advisor from student
    /// </summary>
    [HttpDelete("remove/{studentId}")]
    public async Task<IActionResult> RemoveAdvisorFromStudent(string studentId)
    {
        try
        {
            var student = await _userManager.FindByIdAsync(studentId);
       if (student == null)
      return NotFound(new { error = "Student not found" });

            if (student.AdvisorId == null)
       return BadRequest(new { error = "Student does not have an advisor" });

   var previousAdvisorId = student.AdvisorId;
            
          // Remove advisor
   student.AdvisorId = null;
            var result = await _userManager.UpdateAsync(student);

         if (!result.Succeeded)
            {
         return StatusCode(500, new
                {
             error = "Failed to remove advisor",
           details = result.Errors.Select(e => e.Description)
                });
       }

  // Notify student
            await _notificationService.CreateNotificationAsync(
    student.Id,
     "Öðretmen Atamasý Kaldýrýldý",
     "Öðretmen atamanýz kaldýrýldý.",
 NotificationType.General,
      null,
     null
            );

            // Notify previous advisor
            await _notificationService.CreateNotificationAsync(
     previousAdvisorId,
            "Öðrenci Atamasý Kaldýrýldý",
      $"{student.UserName} artýk öðrenciniz deðil.",
             NotificationType.General,
      null,
null
       );

            _logger.LogInformation(
           "Advisor removed from student {StudentName} by admin",
              student.UserName
            );

            return Ok(new
            {
    message = "Öðretmen atamasý baþarýyla kaldýrýldý",
     studentId = student.Id,
                studentName = student.UserName
         });
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Failed to remove advisor from student");
   return StatusCode(500, new
     {
    error = "Failed to remove advisor",
         details = ex.Message
  });
        }
    }

    /// <summary>
    /// Admin: Get advisor details with assigned students
    /// </summary>
    [HttpGet("{advisorId}")]
    public async Task<IActionResult> GetAdvisorDetails(string advisorId)
    {
    try
        {
            var advisor = await _userManager.FindByIdAsync(advisorId);
   if (advisor == null)
            return NotFound(new { error = "Advisor not found" });

            if (!await _userManager.IsInRoleAsync(advisor, "Advisor"))
   return BadRequest(new { error = "User is not an advisor" });

        // Get assigned students
            var students = await _userManager.Users
    .Where(u => u.AdvisorId == advisorId)
    .Select(s => new
        {
      s.Id,
        s.UserName,
        s.Email,
          s.EmailConfirmed
   })
           .ToListAsync();

            return Ok(new
        {
       id = advisor.Id,
     userName = advisor.UserName,
    email = advisor.Email,
        emailConfirmed = advisor.EmailConfirmed,
         assignedStudentsCount = students.Count,
                students
        });
        }
      catch (Exception ex)
        {
       _logger.LogError(ex, "Failed to get advisor details");
   return StatusCode(500, new { error = "Failed to retrieve advisor details" });
        }
    }

    // DTO
  public record AssignAdvisorRequest(string StudentId, string AdvisorId);
}
