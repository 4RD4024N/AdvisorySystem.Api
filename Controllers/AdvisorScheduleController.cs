using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/advisor-schedule")]
[Authorize(Roles = "Advisor")]
[EnableRateLimiting("standard")]
public class AdvisorScheduleController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<AdvisorScheduleController> _logger;

  public AdvisorScheduleController(
        AppDbContext db,
        UserManager<AppUser> userManager,
        ILogger<AdvisorScheduleController> logger)
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

    /// <summary>
  /// Danýþmanýn öðrencilerini listeler
    /// </summary>
    [HttpGet("my-students")]
    public async Task<IActionResult> GetMyStudents()
    {
      try
        {
            var advisorId = GetUserId();

 var students = await _userManager.Users
.Where(u => u.AdvisorId == advisorId)
  .ToListAsync();

          var studentList = new List<object>();

      foreach (var student in students)
         {
          var roles = await _userManager.GetRolesAsync(student);
       if (!roles.Contains("Student")) continue;

     var profile = await _db.StudentProfiles
          .FirstOrDefaultAsync(sp => sp.UserId == student.Id);

        var enrollmentCount = await _db.StudentCourseSections
  .Where(scs => scs.StudentId == student.Id && !scs.IsCompleted)
        .CountAsync();

         var completedCount = await _db.StudentCourseSections
.Where(scs => scs.StudentId == student.Id && scs.IsCompleted)
               .CountAsync();

     studentList.Add(new
   {
     studentId = student.Id,
       email = student.Email,
    userName = student.UserName,
          firstName = profile?.FirstName,
      lastName = profile?.LastName,
         fullName = !string.IsNullOrEmpty(profile?.FullName) ? profile.FullName : student.Email,
    studentNumber = profile?.StudentNumber,
                    department = profile?.Department,
   gpa = profile?.GPA,
  activeEnrollments = enrollmentCount,
     completedCourses = completedCount,
      hasEnrollments = enrollmentCount > 0
        });
            }

       return Ok(new
   {
             advisorId,
       totalStudents = studentList.Count,
        students = studentList.OrderBy(s => ((dynamic)s).fullName)
     });
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to get students");
     return StatusCode(500, new { error = "Öðrenci listesi alýnamadý", details = ex.Message });
        }
    }

    /// <summary>
 /// Belirli bir öðrencinin ders programýný getirir
 /// </summary>
    [HttpGet("student-schedule/{studentId}")]
    public async Task<IActionResult> GetStudentSchedule(string studentId)
    {
  try
        {
  var advisorId = GetUserId();

            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
     return NotFound(new { error = "Öðrenci bulunamadý" });

    // Sadece kendi öðrencisinin programýný görebilir
    if (student.AdvisorId != advisorId)
       return Forbid();

            var profile = await _db.StudentProfiles
              .FirstOrDefaultAsync(sp => sp.UserId == studentId);

    var enrollments = await _db.StudentCourseSections
      .Where(scs => scs.StudentId == studentId && !scs.IsCompleted)
          .Include(scs => scs.Course)
 .ThenInclude(c => c.Category)
                .ToListAsync();

    var schedule = new List<object>();

      foreach (var enrollment in enrollments)
          {
       var sessions = await _db.CourseSchedules
         .Where(cs => cs.CourseId == enrollment.CourseId && cs.SectionCode == enrollment.SectionCode)
         .OrderBy(cs => cs.DayOfWeek)
       .ThenBy(cs => cs.StartTime)
 .Select(cs => new
         {
      day = cs.DayOfWeek.ToString(),
   dayNumber = (int)cs.DayOfWeek,
               startTime = cs.StartTime.ToString(@"hh\:mm"),
              endTime = cs.EndTime.ToString(@"hh\:mm"),
             cs.RoomNumber,
    cs.InstructorName
    })
     .ToListAsync();

      schedule.Add(new
   {
    enrollmentId = enrollment.Id,
         courseId = enrollment.CourseId,
     courseCode = enrollment.Course.CourseCode,
         courseName = enrollment.Course.CourseName,
          sectionCode = enrollment.SectionCode,
             semester = enrollment.Course.Semester,
     credits = enrollment.Course.Credits,
     ects = enrollment.Course.ECTS,
          category = enrollment.Course.Category?.Name,
     enrolledAt = enrollment.EnrolledAt,
           sessions
                });
        }

  // Haftalýk görünüm için günlere göre grupla
            var weeklyView = BuildWeeklyView(schedule);

            return Ok(new
            {
 student = new
  {
   studentId,
    email = student.Email,
           fullName = !string.IsNullOrEmpty(profile?.FullName) ? profile.FullName : student.Email,
         studentNumber = profile?.StudentNumber,
 department = profile?.Department,
         gpa = profile?.GPA
         },
          totalCourses = enrollments.Count,
      totalCredits = enrollments.Sum(e => e.Course.Credits),
       totalECTS = enrollments.Sum(e => e.Course.ECTS),
       enrollments = schedule,
     weeklySchedule = weeklyView
      });
        }
 catch (Exception ex)
        {
  _logger.LogError(ex, "Failed to get student schedule");
        return StatusCode(500, new { error = "Ders programý alýnamadý", details = ex.Message });
      }
    }

    /// <summary>
    /// Tüm öðrencilerin ders programlarýný özet olarak getirir
    /// </summary>
    [HttpGet("all-schedules")]
    public async Task<IActionResult> GetAllStudentSchedules()
    {
        try
        {
   var advisorId = GetUserId();

            var students = await _userManager.Users
        .Where(u => u.AdvisorId == advisorId)
         .ToListAsync();

            var allSchedules = new List<object>();

          foreach (var student in students)
      {
    var roles = await _userManager.GetRolesAsync(student);
   if (!roles.Contains("Student")) continue;

         var profile = await _db.StudentProfiles
       .FirstOrDefaultAsync(sp => sp.UserId == student.Id);

                var enrollments = await _db.StudentCourseSections
     .Where(scs => scs.StudentId == student.Id && !scs.IsCompleted)
     .Include(scs => scs.Course)
        .ToListAsync();

             if (!enrollments.Any()) continue;

    var courseList = new List<object>();

        foreach (var enrollment in enrollments)
      {
   var sessions = await _db.CourseSchedules
          .Where(cs => cs.CourseId == enrollment.CourseId && cs.SectionCode == enrollment.SectionCode)
         .Select(cs => new
    {
    day = cs.DayOfWeek.ToString(),
    dayNumber = (int)cs.DayOfWeek,
        startTime = cs.StartTime.ToString(@"hh\:mm"),
      endTime = cs.EndTime.ToString(@"hh\:mm"),
     cs.RoomNumber
 })
          .ToListAsync();

      courseList.Add(new
          {
 courseCode = enrollment.Course.CourseCode,
              courseName = enrollment.Course.CourseName,
  sectionCode = enrollment.SectionCode,
        credits = enrollment.Course.Credits,
           sessions
      });
     }

   allSchedules.Add(new
         {
  studentId = student.Id,
    fullName = !string.IsNullOrEmpty(profile?.FullName) ? profile.FullName : student.Email,
  studentNumber = profile?.StudentNumber,
         totalCourses = enrollments.Count,
     totalCredits = enrollments.Sum(e => e.Course.Credits),
         courses = courseList
            });
       }

       return Ok(new
 {
          advisorId,
           totalStudents = allSchedules.Count,
            students = allSchedules.OrderBy(s => ((dynamic)s).fullName)
        });
    }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Failed to get all schedules");
            return StatusCode(500, new { error = "Programlar alýnamadý", details = ex.Message });
        }
    }

    /// <summary>
    /// Belirli bir günde öðrencilerin ders programlarýný getirir
    /// </summary>
    [HttpGet("by-day/{dayOfWeek}")]
    public async Task<IActionResult> GetSchedulesByDay(DayOfWeek dayOfWeek)
    {
        try
        {
 var advisorId = GetUserId();

       var students = await _userManager.Users
          .Where(u => u.AdvisorId == advisorId)
                .ToListAsync();

       var daySchedules = new List<object>();

            foreach (var student in students)
         {
    var roles = await _userManager.GetRolesAsync(student);
   if (!roles.Contains("Student")) continue;

  var profile = await _db.StudentProfiles
             .FirstOrDefaultAsync(sp => sp.UserId == student.Id);

            var enrollments = await _db.StudentCourseSections
              .Where(scs => scs.StudentId == student.Id && !scs.IsCompleted)
       .Include(scs => scs.Course)
         .ToListAsync();

  var studentDayCourses = new List<object>();

         foreach (var enrollment in enrollments)
     {
  var sessions = await _db.CourseSchedules
            .Where(cs => cs.CourseId == enrollment.CourseId 
           && cs.SectionCode == enrollment.SectionCode
         && cs.DayOfWeek == dayOfWeek)
     .OrderBy(cs => cs.StartTime)
      .Select(cs => new
        {
         startTime = cs.StartTime.ToString(@"hh\:mm"),
    endTime = cs.EndTime.ToString(@"hh\:mm"),
            cs.RoomNumber,
            cs.InstructorName
              })
     .ToListAsync();

     if (sessions.Any())
               {
        studentDayCourses.Add(new
          {
                courseCode = enrollment.Course.CourseCode,
     courseName = enrollment.Course.CourseName,
      sectionCode = enrollment.SectionCode,
          sessions
     });
           }
   }

   if (studentDayCourses.Any())
              {
         daySchedules.Add(new
             {
      studentId = student.Id,
       fullName = !string.IsNullOrEmpty(profile?.FullName) ? profile.FullName : student.Email,
      studentNumber = profile?.StudentNumber,
    courses = studentDayCourses.OrderBy(c => 
            ((dynamic)c).sessions[0].startTime)
     });
          }
        }

            return Ok(new
            {
           day = dayOfWeek.ToString(),
    dayNumber = (int)dayOfWeek,
        totalStudents = daySchedules.Count,
     students = daySchedules.OrderBy(s => ((dynamic)s).fullName)
            });
        }
   catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to get schedules by day");
            return StatusCode(500, new { error = "Program alýnamadý", details = ex.Message });
   }
    }

 /// <summary>
    /// Danýþman için istatistikler
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
   {
            var advisorId = GetUserId();

       var students = await _userManager.Users
.Where(u => u.AdvisorId == advisorId)
                .ToListAsync();

            var studentIds = new List<string>();
  foreach (var student in students)
            {
       var roles = await _userManager.GetRolesAsync(student);
                if (roles.Contains("Student"))
         studentIds.Add(student.Id);
            }

       var totalEnrollments = await _db.StudentCourseSections
                .Where(scs => studentIds.Contains(scs.StudentId) && !scs.IsCompleted)
         .CountAsync();

   var totalCompleted = await _db.StudentCourseSections
              .Where(scs => studentIds.Contains(scs.StudentId) && scs.IsCompleted)
        .CountAsync();

            // En çok alýnan dersler
            var popularCourses = await _db.StudentCourseSections
.Where(scs => studentIds.Contains(scs.StudentId) && !scs.IsCompleted)
    .GroupBy(scs => scs.CourseId)
              .Select(g => new
    {
       courseId = g.Key,
          studentCount = g.Count()
        })
     .OrderByDescending(x => x.studentCount)
   .Take(5)
   .ToListAsync();

            var popularCourseDetails = new List<object>();
       foreach (var pc in popularCourses)
        {
         var course = await _db.Courses.FindAsync(pc.courseId);
      if (course != null)
    {
           popularCourseDetails.Add(new
      {
     courseCode = course.CourseCode,
 courseName = course.CourseName,
   studentCount = pc.studentCount
   });
           }
            }

  // Ortalama kredi
   var avgCredits = 0.0;
   if (studentIds.Any())
            {
        var totalCredits = await _db.StudentCourseSections
       .Where(scs => studentIds.Contains(scs.StudentId) && !scs.IsCompleted)
    .Include(scs => scs.Course)
        .SumAsync(scs => scs.Course.Credits);
                avgCredits = (double)totalCredits / studentIds.Count;
            }

    // GPA daðýlýmý
var profiles = await _db.StudentProfiles
    .Where(sp => studentIds.Contains(sp.UserId) && sp.GPA.HasValue)
    .ToListAsync();

var avgGPA = profiles.Any() ? profiles.Average(p => p.GPA ?? 0) : 0;

            return Ok(new
     {
           totalStudents = studentIds.Count,
studentsWithEnrollments = await _db.StudentCourseSections
    .Where(scs => studentIds.Contains(scs.StudentId))
     .Select(scs => scs.StudentId)
          .Distinct()
 .CountAsync(),
             totalActiveEnrollments = totalEnrollments,
       totalCompletedCourses = totalCompleted,
 averageCreditsPerStudent = Math.Round(avgCredits, 1),
                averageGPA = Math.Round(avgGPA, 2),
       popularCourses = popularCourseDetails
            });
  }
        catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get statistics");
    return StatusCode(500, new { error = "Ýstatistikler alýnamadý", details = ex.Message });
        }
    }

    /// <summary>
    /// Haftalýk görünüm oluþturur
    /// </summary>
    private List<object> BuildWeeklyView(List<object> schedule)
    {
    var allSessions = new List<dynamic>();

        foreach (var item in schedule)
        {
 var scheduleItem = (dynamic)item;
    foreach (var session in scheduleItem.sessions)
            {
      allSessions.Add(new
   {
    courseCode = scheduleItem.courseCode,
         courseName = scheduleItem.courseName,
   sectionCode = scheduleItem.sectionCode,
         day = session.day,
        dayNumber = session.dayNumber,
                 startTime = session.startTime,
        endTime = session.endTime,
         roomNumber = session.RoomNumber
        });
            }
        }

        var weekDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        var weeklyView = new List<object>();

        foreach (var day in weekDays)
        {
   var daySessions = allSessions
      .Where(s => s.day == day)
         .OrderBy(s => s.startTime)
             .ToList();

         if (daySessions.Any())
  {
    weeklyView.Add(new
         {
              day,
courses = daySessions
      });
      }
        }

        return weeklyView;
    }
}
