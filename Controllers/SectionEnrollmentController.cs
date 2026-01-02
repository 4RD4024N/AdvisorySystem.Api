using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/section-enrollment")]
[Authorize]
public class SectionEnrollmentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<SectionEnrollmentController> _logger;

    public SectionEnrollmentController(AppDbContext db, ILogger<SectionEnrollmentController> logger)
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

    // NEW: Get all available courses for a semester (before sections are created)
    [HttpGet("available-courses/{semester}")]
    public async Task<IActionResult> GetAvailableCoursesForEnrollment(int semester)
    {
  try
        {
        var userId = GetUserId();

            // Get all courses for this semester
      var courses = await _db.Courses
       .Include(c => c.Category)
       .Where(c => c.Semester == semester && !c.IsElective)
              .OrderBy(c => c.CourseCode)
     .ToListAsync();

 // Get student's current enrollments
    var enrolledCourseIds = await _db.StudentCourseSections
    .Where(scs => scs.StudentId == userId && scs.Semester == semester)
    .Select(scs => scs.CourseId)
       .ToListAsync();

         var availableCourses = courses.Select(c => new
            {
     c.Id,
    c.CourseCode,
        c.CourseName,
   c.TheoryHours,
           c.PracticeHours,
    c.Credits,
      c.ECTS,
    totalWeeklyHours = c.TheoryHours + c.PracticeHours,
      category = c.Category.Name,
           isEnrolled = enrolledCourseIds.Contains(c.Id),
    hasSchedule = _db.CourseSchedules.Any(cs => cs.CourseId == c.Id && cs.Semester == semester)
      }).ToList();

return Ok(new
      {
                semester,
     totalCourses = courses.Count,
       enrolledCount = enrolledCourseIds.Count,
            availableForEnrollment = courses.Count - enrolledCourseIds.Count,
        courses = availableCourses
});
        }
        catch (Exception ex)
     {
        _logger.LogError(ex, "Failed to get available courses");
   return StatusCode(500, new { error = "Failed to retrieve courses", details = ex.Message });
      }
    }

    //  Get available sections for a specific course
    [HttpGet("available-sections/{courseId}/{semester}")]
    public async Task<IActionResult> GetAvailableSections(int courseId, int semester)
    {
     try
        {
 var course = await _db.Courses.FindAsync(courseId);
    if (course == null)
         return NotFound(new { error = "Course not found" });

          // Get all sections for this course
            var sections = await _db.CourseSchedules
         .Where(cs => cs.CourseId == courseId && cs.Semester == semester)
     .GroupBy(cs => cs.SectionCode)
     .Select(g => new
   {
      sectionCode = g.Key,
           sessions = g.OrderBy(s => s.SessionNumber).Select(s => new
        {
      s.Id,
         s.SessionNumber,
 day = s.DayOfWeek.ToString(),
   startTime = s.StartTime.ToString(@"hh\:mm"),
     endTime = s.EndTime.ToString(@"hh\:mm"),
       duration = $"{(int)(s.EndTime - s.StartTime).TotalMinutes} min",
      s.IsTheory,
        s.RoomNumber,
 s.InstructorName,
     s.MaxCapacity
       }).ToList(),
    totalSessions = g.Count(),
        enrolledCount = _db.StudentCourseSections.Count(scs => 
    scs.CourseId == courseId && 
             scs.SectionCode == g.Key && 
                scs.Semester == semester),
               isFull = _db.StudentCourseSections.Count(scs => 
     scs.CourseId == courseId && 
             scs.SectionCode == g.Key && 
        scs.Semester == semester) >= g.First().MaxCapacity
   })
    .ToListAsync();

      return Ok(new
      {
          courseId,
            courseCode = course.CourseCode,
          courseName = course.CourseName,
         semester,
             totalSections = sections.Count,
      sections
            });
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to get available sections");
       return StatusCode(500, new { error = "Failed to retrieve sections", details = ex.Message });
        }
    }

    // Enroll in a course (with or without section)
 [HttpPost("enroll")]
    public async Task<IActionResult> EnrollInCourse([FromBody] EnrollDto dto)
    {
    try
  {
var userId = GetUserId();

 // Check if course exists
 var course = await _db.Courses.FindAsync(dto.CourseId);
  if (course == null)
     return NotFound(new { error = "Course not found" });

         // Check if already enrolled
            var existingEnrollment = await _db.StudentCourseSections
                .FirstOrDefaultAsync(scs => 
            scs.StudentId == userId && 
           scs.CourseId == dto.CourseId && 
  scs.Semester == dto.Semester);

        if (existingEnrollment != null)
             return BadRequest(new { error = "Already enrolled in this course" });

     // Enroll (section will be "TBD" if schedule not generated yet)
            var sectionCode = string.IsNullOrEmpty(dto.SectionCode) ? "TBD" : dto.SectionCode;

         var enrollment = new StudentCourseSection
   {
        StudentId = userId,
           CourseId = dto.CourseId,
         SectionCode = sectionCode,
      Semester = dto.Semester,
     IsCompleted = false
   };

  _db.StudentCourseSections.Add(enrollment);
    await _db.SaveChangesAsync();

            var message = sectionCode == "TBD" 
   ? "Enrolled successfully (section will be assigned when schedule is created)"
         : "Enrolled successfully";

return Ok(new
   {
         message,
    enrollmentId = enrollment.Id,
         sectionCode = enrollment.SectionCode
    });
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Failed to enroll");
            return StatusCode(500, new { error = "Failed to enroll", details = ex.Message });
      }
    }

    public record EnrollDto(
     int CourseId,
        string? SectionCode,
  int Semester
    );
}
