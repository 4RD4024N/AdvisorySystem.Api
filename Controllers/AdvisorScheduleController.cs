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
[Route("api/advisor-schedule")]
[Authorize(Roles = "Advisor,Admin")]
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

    // Öðrencilerimi getir
  [HttpGet("my-students")]
    public async Task<IActionResult> GetMyStudents()
    {
        try
        {
            var advisorId = GetUserId();
     var isAdmin = User.IsInRole("Admin");

            var studentsQuery = _userManager.Users.AsQueryable();

            if (!isAdmin)
 {
           studentsQuery = studentsQuery.Where(u => u.AdvisorId == advisorId);
       }

          var students = await studentsQuery.ToListAsync();

var studentList = new List<object>();

     foreach (var student in students)
 {
                var roles = await _userManager.GetRolesAsync(student);
  if (!roles.Contains("Student")) continue;

          var profile = await _db.StudentProfiles
   .FirstOrDefaultAsync(sp => sp.UserId == student.Id);

           var enrollmentCount = await _db.StudentCourseSections
    .Where(scs => scs.StudentId == student.Id)
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
    totalEnrollments = enrollmentCount,
          completedCourses = completedCount,
                hasEnrollments = enrollmentCount > 0
  });
   }

     return Ok(new
            {
     advisorId,
       isAdmin,
         totalStudents = studentList.Count,
  students = studentList.OrderBy(s => ((dynamic)s).fullName)
    });
        }
catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get students");
          return StatusCode(500, new { error = "Failed to retrieve students", details = ex.Message });
        }
    }

    // Öðrencinin ders programýný getir
    [HttpGet("student-schedule/{studentId}")]
  public async Task<IActionResult> GetStudentSchedule(string studentId)
    {
        try
    {
          var advisorId = GetUserId();
    var isAdmin = User.IsInRole("Admin");

 var student = await _userManager.FindByIdAsync(studentId);
      if (student == null)
return NotFound(new { error = "Student not found" });

     if (!isAdmin && student.AdvisorId != advisorId)
                return Forbid();

     var profile = await _db.StudentProfiles
   .FirstOrDefaultAsync(sp => sp.UserId == studentId);

          var enrollments = await _db.StudentCourseSections
     .Where(scs => scs.StudentId == studentId)
        .Include(scs => scs.Course)
    .ThenInclude(c => c.Category)
      .ToListAsync();

      var schedule = new List<object>();

     foreach (var enrollment in enrollments)
      {
           var sessions = await _db.CourseSchedules
     .Where(cs => cs.CourseId == enrollment.CourseId && cs.SectionCode == enrollment.SectionCode)
          .OrderBy(cs => cs.SessionNumber)
        .Select(cs => new
    {
        cs.SessionNumber,
   day = cs.DayOfWeek.ToString(),
             startTime = cs.StartTime.ToString(@"hh\:mm"),
  endTime = cs.EndTime.ToString(@"hh\:mm"),
      cs.IsTheory,
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
        category = enrollment.Course.Category.Name,
        isCompleted = enrollment.IsCompleted,
    sessions
             });
            }

     // Haftalýk görünüm için günlere göre grupla
       var allSessions = new List<object>();
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
             session.day,
     session.startTime,
             session.endTime,
    session.IsTheory,
       session.RoomNumber,
         session.InstructorName
        });
      }
            }

            var byDay = allSessions.GroupBy(s => ((dynamic)s).day)
              .Select(g => new
           {
          day = g.Key,
        courses = g.OrderBy(s => ((dynamic)s).startTime).ToList()
   })
      .ToList();

            return Ok(new
            {
     student = new
      {
              studentId,
        email = student.Email,
   userName = student.UserName,
       firstName = profile?.FirstName,
     lastName = profile?.LastName,
    fullName = !string.IsNullOrEmpty(profile?.FullName) ? profile.FullName : student.Email,
      studentNumber = profile?.StudentNumber,
     department = profile?.Department,
      gpa = profile?.GPA
 },
       totalCourses = enrollments.Count,
  completedCourses = enrollments.Count(e => e.IsCompleted),
         enrollments = schedule,
    weeklySchedule = byDay
        });
    }
    catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get student schedule");
            return StatusCode(500, new { error = "Failed to retrieve schedule", details = ex.Message });
    }
    }

    // Tüm öðrencilerin özeti
    [HttpGet("all-students-summary")]
    public async Task<IActionResult> GetAllStudentsSummary()
    {
        try
        {
            var advisorId = GetUserId();
            var isAdmin = User.IsInRole("Admin");

            var studentsQuery = _userManager.Users.AsQueryable();

          if (!isAdmin)
            {
      studentsQuery = studentsQuery.Where(u => u.AdvisorId == advisorId);
            }

       var students = await studentsQuery.ToListAsync();

   var summary = new List<object>();

        foreach (var student in students)
        {
          var roles = await _userManager.GetRolesAsync(student);
  if (!roles.Contains("Student")) continue;

         var profile = await _db.StudentProfiles
  .FirstOrDefaultAsync(sp => sp.UserId == student.Id);

    var enrollments = await _db.StudentCourseSections
        .Where(scs => scs.StudentId == student.Id)
         .Include(scs => scs.Course)
   .ToListAsync();

    if (enrollments.Any())
  {
                summary.Add(new
        {
              studentId = student.Id,
                 email = student.Email,
         fullName = !string.IsNullOrEmpty(profile?.FullName) ? profile.FullName : student.Email,
            studentNumber = profile?.StudentNumber,
 totalCourses = enrollments.Count,
    completedCourses = enrollments.Count(e => e.IsCompleted),
          totalCredits = enrollments.Sum(e => e.Course.Credits),
        gpa = profile?.GPA,
   courses = enrollments.Select(e => new
   {
            e.Course.CourseCode,
        e.Course.CourseName,
   e.SectionCode,
           e.IsCompleted
          }).ToList()
      });
     }
            }

          return Ok(new
   {
         totalStudentsWithEnrollments = summary.Count,
      students = summary.OrderBy(s => ((dynamic)s).fullName)
            });
        }
        catch (Exception ex)
        {
_logger.LogError(ex, "Failed to get summary");
 return StatusCode(500, new { error = "Failed to retrieve summary", details = ex.Message });
  }
    }
}
