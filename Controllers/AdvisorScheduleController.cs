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

    // Get list of my students with their enrollment status
    [HttpGet("my-students")]
    public async Task<IActionResult> GetMyStudents([FromQuery] int? semester = null)
    {
        try
 {
        var advisorId = GetUserId();
            var isAdmin = User.IsInRole("Admin");

 // Get students - Admin sees all, Advisor sees only assigned
       var studentsQuery = _userManager.Users.AsQueryable();
   
      if (!isAdmin)
            {
                // Filter by advisor assignment
     studentsQuery = studentsQuery.Where(u => u.AdvisorId == advisorId);
          }

          var students = await studentsQuery.ToListAsync();

    // Get student roles and filter
  var studentList = new List<object>();

    foreach (var student in students)
     {
       var roles = await _userManager.GetRolesAsync(student);
     if (!roles.Contains("Student")) continue;

    // Get profile
     var profile = await _db.StudentProfiles
      .FirstOrDefaultAsync(sp => sp.UserId == student.Id);

     // Get enrollment count
          var enrollmentQuery = _db.StudentCourseSections
.Where(scs => scs.StudentId == student.Id);

       if (semester.HasValue)
  enrollmentQuery = enrollmentQuery.Where(scs => scs.Semester == semester.Value);

      var enrollmentCount = await enrollmentQuery.CountAsync();
       var completedCount = await enrollmentQuery.CountAsync(scs => scs.IsCompleted);

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
                semester = semester,
        students = studentList.OrderBy(s => ((dynamic)s).fullName)
       });
        }
    catch (Exception ex)
        {
 _logger.LogError(ex, "Failed to get students");
      return StatusCode(500, new { error = "Failed to retrieve students", details = ex.Message });
   }
    }

    // Get specific student's schedule
    [HttpGet("student-schedule/{studentId}")]
    public async Task<IActionResult> GetStudentSchedule(string studentId, [FromQuery] int semester)
    {
    try
        {
            var advisorId = GetUserId();
 var isAdmin = User.IsInRole("Admin");

         // Check authorization
    var student = await _userManager.FindByIdAsync(studentId);
   if (student == null)
    return NotFound(new { error = "Student not found" });

   if (!isAdmin && student.AdvisorId != advisorId)
     return Forbid();

            // Get student profile
   var profile = await _db.StudentProfiles
   .FirstOrDefaultAsync(sp => sp.UserId == studentId);

   // Get enrollments
            var enrollments = await _db.StudentCourseSections
   .Where(scs => scs.StudentId == studentId && scs.Semester == semester)
   .Include(scs => scs.Course)
     .ThenInclude(c => c.Category)
     .ToListAsync();

            var schedule = new List<object>();

            foreach (var enrollment in enrollments)
    {
      // Get schedule sessions for this enrollment
           var sessions = await _db.CourseSchedules
    .Where(cs =>
    cs.CourseId == enrollment.CourseId &&
            cs.SectionCode == enrollment.SectionCode &&
      cs.Semester == semester)
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
           credits = enrollment.Course.Credits,
        ects = enrollment.Course.ECTS,
           category = enrollment.Course.Category.Name,
  isCompleted = enrollment.IsCompleted,
     grade = enrollment.Grade,
         letterGrade = enrollment.LetterGrade,
  sessions
             });
        }

     // Group by day for weekly view
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
      semester,
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

    // Get all my students' schedules summary
    [HttpGet("all-students-summary")]
    public async Task<IActionResult> GetAllStudentsSummary([FromQuery] int semester)
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
         .Where(scs => scs.StudentId == student.Id && scs.Semester == semester)
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
  semester,
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
