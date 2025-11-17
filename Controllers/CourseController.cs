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
public class CourseController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CourseController> _logger;

    public CourseController(AppDbContext db, ILogger<CourseController> logger)
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

    // Get all course requirements
  [HttpGet("requirements")]
    public async Task<IActionResult> GetCourseRequirements()
    {
    try
        {
   var courses = await _db.CourseRequirements
     .OrderBy(cr => cr.CourseName)
        .ToListAsync();

            return Ok(courses);
        }
   catch (Exception ex)
  {
  _logger.LogError(ex, "Failed to get course requirements");
 return StatusCode(500, new { error = "Failed to retrieve courses", details = ex.Message });
  }
    }

    // Add course requirement (Admin only)
    [HttpPost("requirements")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddCourseRequirement([FromBody] CourseRequirementDto dto)
    {
        try
        {
         var course = new CourseRequirement
         {
        CourseName = dto.CourseName,
         CourseCode = dto.CourseCode,
    Credits = dto.Credits,
      IsRequired = dto.IsRequired,
    Description = dto.Description
    };

            _db.CourseRequirements.Add(course);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Course requirement added", course.Id });
        }
   catch (Exception ex)
        {
     _logger.LogError(ex, "Failed to add course requirement");
            return StatusCode(500, new { error = "Failed to add course", details = ex.Message });
        }
    }

    // Get my courses (Student)
    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        try
  {
          var userId = GetUserId();
       var courses = await _db.StudentCourses
            .Where(sc => sc.StudentId == userId)
         .Include(sc => sc.CourseRequirement)
    .OrderByDescending(sc => sc.CompletionDate)
  .Select(sc => new
     {
           sc.Id,
  sc.CourseRequirementId,
                 courseName = sc.CourseRequirement.CourseName,
        courseCode = sc.CourseRequirement.CourseCode,
           credits = sc.CourseRequirement.Credits,
   sc.IsCompleted,
       sc.Grade,
           sc.CompletionDate
          })
            .ToListAsync();

   return Ok(courses);
 }
        catch (Exception ex)
 {
            _logger.LogError(ex, "Failed to get student courses");
          return StatusCode(500, new { error = "Failed to retrieve courses", details = ex.Message });
  }
    }

    // Add course to student record
  [HttpPost("my-courses")]
    public async Task<IActionResult> AddMyCourse([FromBody] StudentCourseDto dto)
    {
        try
        {
            var userId = GetUserId();

    // Check if course requirement exists
   var courseReq = await _db.CourseRequirements.FindAsync(dto.CourseRequirementId);
       if (courseReq == null)
    {
  return NotFound(new { error = "Course requirement not found" });
   }

            // Check if already added
            var exists = await _db.StudentCourses
    .AnyAsync(sc => sc.StudentId == userId && sc.CourseRequirementId == dto.CourseRequirementId);

  if (exists)
    {
     return BadRequest(new { error = "Course already added" });
            }

     var studentCourse = new StudentCourse
            {
    StudentId = userId,
                CourseRequirementId = dto.CourseRequirementId,
                IsCompleted = dto.IsCompleted,
  Grade = dto.Grade,
      CompletionDate = dto.CompletionDate
            };

   _db.StudentCourses.Add(studentCourse);
       await _db.SaveChangesAsync();

 // Update student profile credits
  var profile = await _db.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
       if (profile != null && dto.IsCompleted)
            {
 profile.CompletedCredits = (profile.CompletedCredits ?? 0) + courseReq.Credits;
     profile.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();
 }

     return Ok(new { message = "Course added successfully", studentCourse.Id });
        }
        catch (Exception ex)
    {
       _logger.LogError(ex, "Failed to add student course");
      return StatusCode(500, new { error = "Failed to add course", details = ex.Message });
 }
    }

 // Update course completion status
    [HttpPatch("my-courses/{id}")]
    public async Task<IActionResult> UpdateCourseCompletion(int id, [FromBody] UpdateCourseDto dto)
    {
   try
        {
            var userId = GetUserId();
     var studentCourse = await _db.StudentCourses
      .Include(sc => sc.CourseRequirement)
  .FirstOrDefaultAsync(sc => sc.Id == id && sc.StudentId == userId);

   if (studentCourse == null)
       {
      return NotFound(new { error = "Course not found" });
     }

     var wasCompleted = studentCourse.IsCompleted;
    studentCourse.IsCompleted = dto.IsCompleted;
     studentCourse.Grade = dto.Grade ?? studentCourse.Grade;
          studentCourse.CompletionDate = dto.CompletionDate ?? studentCourse.CompletionDate;

  await _db.SaveChangesAsync();

            // Update student profile credits
            if (dto.IsCompleted && !wasCompleted)
       {
    var profile = await _db.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
      if (profile != null)
 {
   profile.CompletedCredits = (profile.CompletedCredits ?? 0) + studentCourse.CourseRequirement.Credits;
    profile.UpdatedAt = DateTime.UtcNow;
      await _db.SaveChangesAsync();
 }
            }

    return Ok(new { message = "Course updated successfully" });
        }
        catch (Exception ex)
        {
         _logger.LogError(ex, "Failed to update course");
  return StatusCode(500, new { error = "Failed to update course", details = ex.Message });
        }
    }

    // DTOs
    public record CourseRequirementDto(
        string CourseName,
        string? CourseCode,
      int Credits,
   bool IsRequired,
        string? Description
    );

    public record StudentCourseDto(
    int CourseRequirementId,
        bool IsCompleted,
        double? Grade,
      DateTime? CompletionDate
    );

    public record UpdateCourseDto(
        bool IsCompleted,
        double? Grade,
    DateTime? CompletionDate
    );
}
