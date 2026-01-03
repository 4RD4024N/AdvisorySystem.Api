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

    // ? ADMIN ERIÞEMEZ - Sadece Student/Advisor
    [HttpGet("my-program")]
    [Authorize(Roles = "Student,Advisor")]
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

    // ? Advisor öðrencilerinin programýný görebilir, Admin YAPAMAZ
    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Advisor")]
  public async Task<IActionResult> GetStudentProgram(string studentId)
    {
   try
        {
            var currentUserId = GetUserId();

   var student = await _userManager.FindByIdAsync(studentId);
      if (student == null)
     return NotFound(new { error = "Student not found" });

   // Advisor sadece kendi öðrencilerini görebilir
       if (student.AdvisorId != currentUserId)
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

    // ? ADMIN ERIÞEMEZ - Sadece Student
    [HttpPost("enroll")]
    [Authorize(Roles = "Student")]
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

 // ? Dersin schedule'ýný kontrol et
       var scheduleExists = await _db.CourseSchedules
                .AnyAsync(cs => cs.CourseId == dto.CourseId && cs.Semester == (dto.Semester ?? 0));

            if (!scheduleExists)
            {
             _logger.LogWarning($"Course {dto.CourseId} has no schedule for semester {dto.Semester}");
  return BadRequest(new { 
           error = "Course schedule not found",
       message = $"This course doesn't have a schedule for semester {dto.Semester}. Please contact admin to generate schedules."
    });
     }

      var studentCourse = new StudentCourse
            {
           StudentId = userId,
      CourseId = dto.CourseId,
      Semester = dto.Semester,
                IsCompleted = false
            };

            _db.StudentCourses.Add(studentCourse);

      // ? StudentCourseSection'a da ekle (ders programýnda görmek için)
      var defaultSection = await _db.CourseSchedules
          .Where(cs => cs.CourseId == dto.CourseId && cs.Semester == (dto.Semester ?? 0))
   .OrderBy(cs => cs.SectionCode)
       .FirstOrDefaultAsync();

            if (defaultSection != null)
            {
     var section = new StudentCourseSection
            {
 StudentId = userId,
      CourseId = dto.CourseId,
            SectionCode = defaultSection.SectionCode,
    Semester = dto.Semester ?? 0,
          IsCompleted = false
                };
     _db.StudentCourseSections.Add(section);
    }

       await _db.SaveChangesAsync();

          return Ok(new
 {
         message = "Enrolled successfully",
      enrollmentId = studentCourse.Id,
            sectionCode = defaultSection?.SectionCode,
                semester = dto.Semester
            });
        }
        catch (Exception ex)
  {
            _logger.LogError(ex, "Failed to enroll in course");
      return StatusCode(500, new { error = "Failed to enroll", details = ex.Message });
  }
    }

    // ? ADMIN ERIÞEMEZ - Sadece Student
    [HttpGet("my-schedule")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySchedule([FromQuery] int? semester = null)
    {
        try
{
       var userId = GetUserId();

   var query = _db.StudentCourseSections
       .Where(scs => scs.StudentId == userId);

    if (semester.HasValue)
    query = query.Where(scs => scs.Semester == semester.Value);

            var enrolledSections = await query
   .Include(scs => scs.Course)
 .ThenInclude(c => c.Category)
    .ToListAsync();

      var scheduleDetails = new List<object>();

  foreach (var section in enrolledSections)
 {
            var schedules = await _db.CourseSchedules
     .Where(cs => cs.CourseId == section.CourseId 
 && cs.SectionCode == section.SectionCode
 && cs.Semester == section.Semester)
     .OrderBy(cs => cs.DayOfWeek)
 .ThenBy(cs => cs.StartTime)
       .ToListAsync();

       if (schedules.Any())
   {
      scheduleDetails.Add(new
   {
  courseId = section.CourseId,
  courseCode = section.Course.CourseCode,
      courseName = section.Course.CourseName,
    description = section.Course.Description,
       credits = section.Course.Credits,
        ects = section.Course.ECTS,
  category = section.Course.Category.Name,
    sectionCode = section.SectionCode,
        semester = section.Semester,
  isCompleted = section.IsCompleted,
    grade = section.Grade,
     sessions = schedules.Select(s => new
        {
           scheduleId = s.Id,
      dayOfWeek = s.DayOfWeek.ToString(),
 dayOfWeekNumber = (int)s.DayOfWeek,
   dayName = s.DayOfWeek switch
           {
   DayOfWeek.Monday => "Pazartesi",
            DayOfWeek.Tuesday => "Salý",
  DayOfWeek.Wednesday => "Çarþamba",
    DayOfWeek.Thursday => "Perþembe",
   DayOfWeek.Friday => "Cuma",
    _ => s.DayOfWeek.ToString()
             },
        startTime = s.StartTime.ToString(@"hh\:mm"),
  endTime = s.EndTime.ToString(@"hh\:mm"),
    timeSlot = $"{s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}",
 roomNumber = s.RoomNumber,
  instructorName = s.InstructorName,
  isTheory = s.IsTheory,
 sessionType = s.IsTheory ? "Teori" : "Uygulama",
        sessionNumber = s.SessionNumber,
       durationMinutes = (int)(s.EndTime - s.StartTime).TotalMinutes
           }).ToList()
      });
   }
        }

            // Haftalýk program (günlere göre)
       var weeklySchedule = new Dictionary<string, object>();
       var dayNames = new Dictionary<DayOfWeek, string>
     {
       { DayOfWeek.Monday, "Pazartesi" },
      { DayOfWeek.Tuesday, "Salý" },
     { DayOfWeek.Wednesday, "Çarþamba" },
    { DayOfWeek.Thursday, "Perþembe" },
    { DayOfWeek.Friday, "Cuma" }
            };

   foreach (var kvp in dayNames)
{
     var day = kvp.Key;
        var dayName = kvp.Value;
     var daySchedule = new List<object>();

      foreach (var courseObj in scheduleDetails)
   {
var courseDict = courseObj as IDictionary<string, object> 
         ?? new Dictionary<string, object>();
            
  if (courseDict.TryGetValue("sessions", out var sessionsObj))
   {
  var sessions = sessionsObj as IEnumerable<object> ?? Enumerable.Empty<object>();
     
     foreach (var sessionObj in sessions)
   {
                 var sessionDict = sessionObj as IDictionary<string, object> 
     ?? new Dictionary<string, object>();

if (sessionDict.TryGetValue("dayOfWeek", out var sessionDay) && 
  sessionDay?.ToString() == day.ToString())
{
  daySchedule.Add(new
  {
 courseId = courseDict.ContainsKey("courseId") ? courseDict["courseId"] : null,
   courseCode = courseDict.ContainsKey("courseCode") ? courseDict["courseCode"] : null,
courseName = courseDict.ContainsKey("courseName") ? courseDict["courseName"] : null,
       sectionCode = courseDict.ContainsKey("sectionCode") ? courseDict["sectionCode"] : null,
     startTime = sessionDict.ContainsKey("startTime") ? sessionDict["startTime"] : null,
     endTime = sessionDict.ContainsKey("endTime") ? sessionDict["endTime"] : null,
timeSlot = sessionDict.ContainsKey("timeSlot") ? sessionDict["timeSlot"] : null,
    roomNumber = sessionDict.ContainsKey("roomNumber") ? sessionDict["roomNumber"] : null,
  instructorName = sessionDict.ContainsKey("instructorName") ? sessionDict["instructorName"] : null,
isTheory = sessionDict.ContainsKey("isTheory") ? sessionDict["isTheory"] : null,
         sessionType = sessionDict.ContainsKey("sessionType") ? sessionDict["sessionType"] : null,
    durationMinutes = sessionDict.ContainsKey("durationMinutes") ? sessionDict["durationMinutes"] : null
     });
       }
        }
      }
  }

 weeklySchedule[dayName] = daySchedule.OrderBy(s => 
{
 var dict = s as IDictionary<string, object>;
      return dict != null && dict.ContainsKey("startTime") ? dict["startTime"]?.ToString() ?? "" : "";
     }).ToList();
}

            // Ýstatistikler
  var totalCredits = enrolledSections.Sum(s => s.Course.Credits);
var totalECTS = enrolledSections.Sum(s => s.Course.ECTS);
   var completedCount = enrolledSections.Count(s => s.IsCompleted);

  return Ok(new
  {
     totalCourses = enrolledSections.Count,
       completedCourses = completedCount,
  totalCredits,
        totalECTS,
   semester = semester.HasValue ? semester.Value.ToString() : "Tümü",
            courses = scheduleDetails,
    weeklySchedule
       });
        }
 catch (Exception ex)
        {
_logger.LogError(ex, "Failed to get student schedule");
  return StatusCode(500, new { error = "Failed to retrieve schedule", details = ex.Message });
}
    }

    // ? ADMIN ERIÞEMEZ - Sadece Student
  [HttpPatch("{enrollmentId}/complete")]
    [Authorize(Roles = "Student")]
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

    // ? ADMIN ERIÞEMEZ - Sadece Student
    [HttpDelete("{enrollmentId}")]
    [Authorize(Roles = "Student")]
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
