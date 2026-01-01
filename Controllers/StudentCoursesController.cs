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
[Route("api/student-courses")]
[Authorize]
public class StudentCoursesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<StudentCoursesController> _logger;

    public StudentCoursesController(
    AppDbContext db,
        UserManager<AppUser> userManager,
        ILogger<StudentCoursesController> logger)
    {
        _db = db;
        _userManager = userManager;
_logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
  ?? User.FindFirstValue("sub")
 ?? throw new UnauthorizedAccessException("User ID not found");
    }

    [HttpGet("my-program")]
    public async Task<IActionResult> GetMyProgram()
  {
        try
   {
       var userId = GetUserId();
      
            var enrolledCourses = await _db.StudentCourses
      .Where(sc => sc.StudentId == userId)
          .Include(sc => sc.Course)
          .ThenInclude(c => c.Category)
    .OrderBy(sc => sc.Semester)
  .ThenBy(sc => sc.Course.CourseCode)
          .Select(sc => new
{
         sc.Id,
           sc.Semester,
          courseId = sc.Course.Id,
    courseCode = sc.Course.CourseCode,
           courseName = sc.Course.CourseName,
         theoryHours = sc.Course.TheoryHours,
 practiceHours = sc.Course.PracticeHours,
           credits = sc.Course.Credits,
    ects = sc.Course.ECTS,
     isElective = sc.Course.IsElective,
         category = sc.Course.Category.Name,
            sc.IsCompleted,
        sc.Grade,
             sc.LetterGrade,
   sc.CompletionDate,
   sc.EnrolledAt
      })
    .ToListAsync();

         var totalCredits = enrolledCourses.Where(c => c.IsCompleted).Sum(c => c.credits);
     var totalECTS = enrolledCourses.Where(c => c.IsCompleted).Sum(c => c.ects);
            var gpa = enrolledCourses.Where(c => c.Grade.HasValue).Average(c => c.Grade);

            return Ok(new
            {
    totalCourses = enrolledCourses.Count,
       completedCourses = enrolledCourses.Count(c => c.IsCompleted),
             totalCredits,
           totalECTS,
         gpa = gpa.HasValue ? Math.Round(gpa.Value, 2) : (double?)null,
                courses = enrolledCourses
            });
        }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Failed to get student program");
            return StatusCode(500, new { error = "Failed to retrieve program", details = ex.Message });
        }
  }

    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Admin,Advisor")]
  public async Task<IActionResult> GetStudentProgram(string studentId)
    {
   try
        {
            var currentUserId = GetUserId();
      var isAdmin = User.IsInRole("Admin");
   var isAdvisor = User.IsInRole("Advisor");

    var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
    return NotFound(new { error = "Student not found" });

  if (isAdvisor && !isAdmin && student.AdvisorId != currentUserId)
     return Forbid();

            var enrolledCourses = await _db.StudentCourses
           .Where(sc => sc.StudentId == studentId)
      .Include(sc => sc.Course)
  .ThenInclude(c => c.Category)
 .OrderBy(sc => sc.Semester)
        .ThenBy(sc => sc.Course.CourseCode)
     .Select(sc => new
   {
        sc.Id,
          sc.Semester,
       courseId = sc.Course.Id,
        courseCode = sc.Course.CourseCode,
         courseName = sc.Course.CourseName,
      theoryHours = sc.Course.TheoryHours,
        practiceHours = sc.Course.PracticeHours,
 credits = sc.Course.Credits,
       ects = sc.Course.ECTS,
   isElective = sc.Course.IsElective,
    category = sc.Course.Category.Name,
        sc.IsCompleted,
         sc.Grade,
      sc.LetterGrade,
      sc.CompletionDate,
        sc.EnrolledAt
    })
        .ToListAsync();

   var totalCredits = enrolledCourses.Where(c => c.IsCompleted).Sum(c => c.credits);
            var totalECTS = enrolledCourses.Where(c => c.IsCompleted).Sum(c => c.ects);
  var gpa = enrolledCourses.Where(c => c.Grade.HasValue).Average(c => c.Grade);

            return Ok(new
 {
        studentId,
             studentName = student.UserName,
 studentEmail = student.Email,
     totalCourses = enrolledCourses.Count,
    completedCourses = enrolledCourses.Count(c => c.IsCompleted),
    totalCredits,
        totalECTS,
            gpa = gpa.HasValue ? Math.Round(gpa.Value, 2) : (double?)null,
           courses = enrolledCourses
        });
   }
    catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get student program");
            return StatusCode(500, new { error = "Failed to retrieve program", details = ex.Message });
   }
    }

    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollCourse([FromBody] EnrollCourseDto dto)
    {
try
        {
            var userId = GetUserId();

  var course = await _db.Courses.FindAsync(dto.CourseId);
     if (course == null)
     return NotFound(new { error = "Course not found" });

      var exists = await _db.StudentCourses
 .AnyAsync(sc => sc.StudentId == userId && sc.CourseId == dto.CourseId);

       if (exists)
        return BadRequest(new { error = "Already enrolled in this course" });

            var studentCourse = new StudentCourse
       {
StudentId = userId,
      CourseId = dto.CourseId,
  Semester = dto.Semester,
         IsCompleted = false
            };

         _db.StudentCourses.Add(studentCourse);
  await _db.SaveChangesAsync();

      return Ok(new
            {
      message = "Enrolled successfully",
       enrollmentId = studentCourse.Id
     });
        }
        catch (Exception ex)
     {
     _logger.LogError(ex, "Failed to enroll in course");
            return StatusCode(500, new { error = "Failed to enroll", details = ex.Message });
      }
    }

    [HttpPatch("{enrollmentId}/complete")]
    public async Task<IActionResult> CompleteCourse(
        int enrollmentId,
        [FromBody] CompleteCourseDto dto)
    {
        try
        {
      var userId = GetUserId();
var enrollment = await _db.StudentCourses
      .Include(sc => sc.Course)
      .FirstOrDefaultAsync(sc => sc.Id == enrollmentId && sc.StudentId == userId);

         if (enrollment == null)
         return NotFound(new { error = "Enrollment not found" });

            enrollment.IsCompleted = true;
            enrollment.Grade = dto.Grade;
      enrollment.LetterGrade = dto.LetterGrade;
            enrollment.CompletionDate = dto.CompletionDate ?? DateTime.UtcNow;

            await _db.SaveChangesAsync();

     var profile = await _db.StudentProfiles
             .FirstOrDefaultAsync(sp => sp.UserId == userId);

  if (profile != null)
         {
      var completedCredits = await _db.StudentCourses
 .Where(sc => sc.StudentId == userId && sc.IsCompleted)
        .Include(sc => sc.Course)
         .SumAsync(sc => sc.Course.Credits);

                profile.CompletedCredits = completedCredits;
        profile.UpdatedAt = DateTime.UtcNow;

     var allGrades = await _db.StudentCourses
    .Where(sc => sc.StudentId == userId && sc.Grade.HasValue)
       .Select(sc => sc.Grade!.Value)
         .ToListAsync();

       if (allGrades.Any())
     {
            profile.GPA = Math.Round(allGrades.Average(), 2);
     }

   await _db.SaveChangesAsync();
            }

            return Ok(new { message = "Course completed successfully" });
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Failed to complete course");
  return StatusCode(500, new { error = "Failed to complete course", details = ex.Message });
        }
    }

    [HttpDelete("{enrollmentId}")]
    public async Task<IActionResult> Unenroll(int enrollmentId)
    {
        try
        {
            var userId = GetUserId();
var enrollment = await _db.StudentCourses
    .FirstOrDefaultAsync(sc => sc.Id == enrollmentId && sc.StudentId == userId);

            if (enrollment == null)
  return NotFound(new { error = "Enrollment not found" });

            if (enrollment.IsCompleted)
        return BadRequest(new { error = "Cannot unenroll from completed course" });

  _db.StudentCourses.Remove(enrollment);
        await _db.SaveChangesAsync();

    return Ok(new { message = "Unenrolled successfully" });
   }
     catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to unenroll");
       return StatusCode(500, new { error = "Failed to unenroll", details = ex.Message });
        }
    }

    public record EnrollCourseDto(
 int CourseId,
        int? Semester
    );

    public record CompleteCourseDto(
        double? Grade,
        string? LetterGrade,
        DateTime? CompletionDate
  );
}
