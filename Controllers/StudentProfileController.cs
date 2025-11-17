using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentProfileController : ControllerBase
{
  private readonly AppDbContext _db;
    private readonly ILogger<StudentProfileController> _logger;

    public StudentProfileController(AppDbContext db, ILogger<StudentProfileController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
   ?? User.FindFirstValue("sub")
   ?? throw new UnauthorizedAccessException("User ID not found");
    }

    // Get my profile
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
  var userId = GetUserId();
            var profile = await _db.StudentProfiles
    .Include(sp => sp.User)
             .FirstOrDefaultAsync(sp => sp.UserId == userId);

if (profile == null)
  {
                return NotFound(new { error = "Profile not found" });
      }

        return Ok(new
            {
         profile.Id,
   profile.UserId,
    userName = profile.User?.UserName,
      profile.StudentNumber,
      profile.Department,
         profile.GPA,
         profile.CompletedCredits,
      profile.EnrollmentDate,
     profile.MeetsPrerequisites,
           profile.CreatedAt,
         profile.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get student profile");
            return StatusCode(500, new { error = "Failed to retrieve profile", details = ex.Message });
    }
    }

    // Create or update profile
    [HttpPost]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> CreateOrUpdateProfile([FromBody] StudentProfileDto dto)
    {
 try
        {
       var userId = GetUserId();
         var profile = await _db.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);

         if (profile == null)
            {
    // Create new profile
                profile = new StudentProfile
        {
         UserId = userId,
  StudentNumber = dto.StudentNumber,
    Department = dto.Department,
        GPA = dto.GPA,
       CompletedCredits = dto.CompletedCredits,
        EnrollmentDate = dto.EnrollmentDate ?? DateTime.UtcNow
       };
     _db.StudentProfiles.Add(profile);
         }
  else
    {
         // Update existing profile
      profile.StudentNumber = dto.StudentNumber ?? profile.StudentNumber;
   profile.Department = dto.Department ?? profile.Department;
              profile.GPA = dto.GPA ?? profile.GPA;
         profile.CompletedCredits = dto.CompletedCredits ?? profile.CompletedCredits;
    profile.EnrollmentDate = dto.EnrollmentDate ?? profile.EnrollmentDate;
     profile.UpdatedAt = DateTime.UtcNow;
}

  await _db.SaveChangesAsync();

            return Ok(new
      {
  message = "Profile saved successfully",
      profile.Id,
        profile.UserId
});
  }
        catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to save student profile");
        return StatusCode(500, new { error = "Failed to save profile", details = ex.Message });
        }
    }

    // Get profile by student ID (Admin/Advisor)
    [HttpGet("{studentId}")]
 [Authorize(Roles = "Admin,Advisor")]
    public async Task<IActionResult> GetProfileByStudentId(string studentId)
    {
        try
   {
      var profile = await _db.StudentProfiles
        .Include(sp => sp.User)
      .FirstOrDefaultAsync(sp => sp.UserId == studentId);

if (profile == null)
       {
       return NotFound(new { error = "Profile not found for this student" });
        }

        return Ok(new
   {
    profile.Id,
      profile.UserId,
     userName = profile.User?.UserName,
        email = profile.User?.Email,
       profile.StudentNumber,
       profile.Department,
      profile.GPA,
 profile.CompletedCredits,
       profile.EnrollmentDate,
                profile.MeetsPrerequisites,
          profile.CreatedAt,
      profile.UpdatedAt
    });
        }
        catch (Exception ex)
    {
     _logger.LogError(ex, "Failed to get student profile");
    return StatusCode(500, new { error = "Failed to retrieve profile", details = ex.Message });
        }
    }

  // Check if student meets prerequisites
    [HttpGet("check-prerequisites")]
    public async Task<IActionResult> CheckPrerequisites()
    {
        try
        {
 var userId = GetUserId();
     var profile = await _db.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null)
       {
              return NotFound(new { error = "Profile not found. Please create a profile first." });
      }

  // Get required courses
 var requiredCourses = await _db.CourseRequirements
 .Where(cr => cr.IsRequired)
    .ToListAsync();

     // Get student's completed courses
 var completedCourses = await _db.StudentCourses
        .Where(sc => sc.StudentId == userId && sc.IsCompleted)
                .Include(sc => sc.CourseRequirement)
        .ToListAsync();

            var totalRequiredCredits = requiredCourses.Sum(rc => rc.Credits);
    var completedCredits = profile.CompletedCredits ?? 0;

            // Check if prerequisites are met
         var meetsPrerequisites = completedCredits >= totalRequiredCredits;

            // Update profile
          profile.MeetsPrerequisites = meetsPrerequisites;
       profile.UpdatedAt = DateTime.UtcNow;
      await _db.SaveChangesAsync();

   return Ok(new
        {
     meetsPrerequisites,
                completedCredits,
            requiredCredits = totalRequiredCredits,
     completedCoursesCount = completedCourses.Count,
    requiredCoursesCount = requiredCourses.Count,
 missingCredits = Math.Max(0, totalRequiredCredits - completedCredits),
          gpa = profile.GPA,
     message = meetsPrerequisites
      ? "? You meet all prerequisites!"
          : $"? Missing {totalRequiredCredits - completedCredits} credits"
      });
  }
        catch (Exception ex)
  {
          _logger.LogError(ex, "Failed to check prerequisites");
            return StatusCode(500, new { error = "Failed to check prerequisites", details = ex.Message });
        }
  }

// DTOs
  public record StudentProfileDto(
        string? StudentNumber,
        string? Department,
        double? GPA,
        int? CompletedCredits,
        DateTime? EnrollmentDate
    );
}
