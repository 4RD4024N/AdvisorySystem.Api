using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/student-courses")]
[Authorize]
[EnableRateLimiting("standard")]
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

    // ��rencinin kendi program�n� getir
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
     .OrderBy(sc => sc.Course.Semester)
   .ThenBy(sc => sc.Course.CourseCode)
    .Select(sc => new
        {
          sc.Id,
semester = sc.Course.Semester,
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
  var gpa = enrolledCourses.Where(c => c.Grade.HasValue).DefaultIfEmpty().Average(c => c?.Grade);

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
    [Authorize(Roles = "Advisor")]
    public async Task<IActionResult> GetStudentProgram(string studentId)
    {
        try
    {
 var currentUserId = GetUserId();

         var student = await _userManager.FindByIdAsync(studentId);
   if (student == null)
                return NotFound(new { error = "Student not found" });

            if (student.AdvisorId != currentUserId)
          return Forbid();

     // ��rencinin kay�tl� derslerini ve section'lar�n� al
            var enrolledSections = await _db.StudentCourseSections
    .Where(scs => scs.StudentId == studentId && !scs.IsCompleted)
   .Include(scs => scs.Course)
          .ThenInclude(c => c.Category)
      .ToListAsync();

         var scheduleDetails = new List<object>();

            // Her ders i�in schedule bilgilerini al
     foreach (var section in enrolledSections)
            {
   var schedules = await _db.CourseSchedules
        .Where(cs => cs.CourseId == section.CourseId && cs.SectionCode == section.SectionCode)
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
        semester = section.Course.Semester,
     isCompleted = section.IsCompleted,
 sessions = schedules.Select(s => new
         {
        scheduleId = s.Id,
         dayOfWeek = s.DayOfWeek.ToString(),
          dayOfWeekNumber = (int)s.DayOfWeek,
   dayName = s.DayOfWeek switch
       {
          DayOfWeek.Monday => "Pazartesi",
         DayOfWeek.Tuesday => "Sali",        // ı yerine i
      DayOfWeek.Wednesday => "Carsamba",  // ş yerine s, ç yerine c
          DayOfWeek.Thursday => "Persembe",   // ş yerine s
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
          durationMinutes = (int)(s.EndTime - s.StartTime).TotalMinutes
         }).ToList()
        });
            }
 }

            // Haftal�k program g�r�n�m�
         var weeklySchedule = new Dictionary<string, List<object>>
     {
    { "Pazartesi", new List<object>() },
    { "Sali", new List<object>() },      // Salı → Sali
    { "Carsamba", new List<object>() },  // Çarşamba → Carsamba
    { "Persembe", new List<object>() },  // Perşembe → Persembe
                { "Cuma", new List<object>() }
            };

    foreach (var section in enrolledSections)
   {
     var schedules = await _db.CourseSchedules
 .Where(cs => cs.CourseId == section.CourseId && cs.SectionCode == section.SectionCode)
    .ToListAsync();

       foreach (var schedule in schedules)
    {
     var dayName = schedule.DayOfWeek switch
     {
     DayOfWeek.Monday => "Pazartesi",
    DayOfWeek.Tuesday => "Sali",
        DayOfWeek.Wednesday => "Carsamba",
                 DayOfWeek.Thursday => "Persembe",
     DayOfWeek.Friday => "Cuma",
               _ => null
          };

  if (dayName != null && weeklySchedule.ContainsKey(dayName))
      {
       weeklySchedule[dayName].Add(new
             {
        courseCode = section.Course.CourseCode,
           courseName = section.Course.CourseName,
    sectionCode = section.SectionCode,
            startTime = schedule.StartTime.ToString(@"hh\:mm"),
        endTime = schedule.EndTime.ToString(@"hh\:mm"),
     timeSlot = $"{schedule.StartTime:hh\\:mm}-{schedule.EndTime:hh\\:mm}",
            roomNumber = schedule.RoomNumber,
         instructorName = schedule.InstructorName,
                sessionType = schedule.IsTheory ? "Teori" : "Uygulama"
       });
   }
                }
     }

            // G�nleri saate g�re s�rala
    foreach (var day in weeklySchedule.Keys.ToList())
 {
          weeklySchedule[day] = weeklySchedule[day]
           .OrderBy(x => ((dynamic)x).startTime)
            .ToList();
   }

        // Tamamlanan dersleri de al (�zet i�in)
      var completedCourses = await _db.StudentCourses
         .Where(sc => sc.StudentId == studentId && sc.IsCompleted)
      .Include(sc => sc.Course)
.ToListAsync();

            var totalCredits = enrolledSections.Sum(s => s.Course.Credits);
      var totalECTS = enrolledSections.Sum(s => s.Course.ECTS);
    var totalCompletedCredits = completedCourses.Sum(c => c.Course.Credits);

        // Student profile bilgisi
            var studentProfile = await _db.StudentProfiles
      .FirstOrDefaultAsync(sp => sp.UserId == studentId);

       return Ok(new
          {
        student = new
 {
    studentId,
         userName = student.UserName,
    email = student.Email,
   fullName = studentProfile?.FullName ?? student.UserName,
         studentNumber = studentProfile?.StudentNumber,
     department = studentProfile?.Department,
   gpa = studentProfile?.GPA
    },
       totalCourses = enrolledSections.Count,
   completedCourses = completedCourses.Count,
                totalCredits,
        totalECTS,
          totalCompletedCredits,
                courses = scheduleDetails,
       weeklySchedule
            });
      }
        catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to get student program");
            return StatusCode(500, new { error = "Failed to retrieve program", details = ex.Message });
        }
    }

    // Derse kay�t ol
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

            // Schedule var m� kontrol et
       var scheduleExists = await _db.CourseSchedules
            .AnyAsync(cs => cs.CourseId == dto.CourseId);

       if (!scheduleExists)
      {
     _logger.LogWarning($"Course {dto.CourseId} has no schedule");
   return BadRequest(new
  {
 error = "Course schedule not found",
         message = "This course doesn't have a schedule. Please contact admin to generate schedules."
     });
         }

  // En uygun section'� bul
            var bestSection = await FindBestSectionAsync(userId, dto.CourseId);

      if (bestSection == null)
          {
           return BadRequest(new
   {
    error = "No available section",
   message = "All sections are either full or conflict with your existing schedule."
});
     }

    var studentCourse = new StudentCourse
        {
          StudentId = userId,
    CourseId = dto.CourseId,
          Semester = course.Semester,
         IsCompleted = false
   };

 _db.StudentCourses.Add(studentCourse);

            // StudentCourseSection'a da ekle
var section = new StudentCourseSection
     {
            StudentId = userId,
      CourseId = dto.CourseId,
SectionCode = bestSection.SectionCode,
  IsCompleted = false
      };
     _db.StudentCourseSections.Add(section);

            await _db.SaveChangesAsync();

return Ok(new
     {
        message = "Enrolled successfully",
         enrollmentId = studentCourse.Id,
 sectionCode = bestSection.SectionCode,
          scheduleInfo = new
  {
      sessions = bestSection.Sessions,
     instructor = bestSection.Instructor
   }
            });
   }
     catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to enroll in course");
      return StatusCode(500, new { error = "Failed to enroll", details = ex.Message });
        }
    }

    // En uygun section'� bul
    private async Task<BestSectionResult?> FindBestSectionAsync(string studentId, int courseId)
    {
        _logger.LogInformation($"Finding best section for student {studentId}, course {courseId}");

   var allSchedules = await _db.CourseSchedules
      .Where(cs => cs.CourseId == courseId)
   .ToListAsync();

        if (!allSchedules.Any())
        {
            _logger.LogWarning($"No schedules found for course {courseId}");
       return null;
        }

        var sections = allSchedules.GroupBy(cs => cs.SectionCode).ToList();

        // ��rencinin mevcut schedule'lar�n� al
        var studentEnrollments = await _db.StudentCourseSections
      .Where(scs => scs.StudentId == studentId && !scs.IsCompleted)
            .ToListAsync();

    var studentScheduleList = new List<CourseSchedule>();
        foreach (var enrollment in studentEnrollments)
     {
       var schedules = await _db.CourseSchedules
       .Where(cs => cs.CourseId == enrollment.CourseId && cs.SectionCode == enrollment.SectionCode)
        .ToListAsync();
       studentScheduleList.AddRange(schedules);
      }

   foreach (var sectionGroup in sections)
   {
    var sectionCode = sectionGroup.Key;
   var sectionSchedules = sectionGroup.ToList();

   // Kapasite kontrol�
    var enrolledCount = await _db.StudentCourseSections
              .CountAsync(scs => scs.CourseId == courseId && scs.SectionCode == sectionCode);

     var maxCapacity = sectionSchedules.First().MaxCapacity;

  if (enrolledCount >= maxCapacity)
            {
      _logger.LogInformation($"Section {sectionCode} is FULL ({enrolledCount}/{maxCapacity})");
    continue;
  }

   // �ak��ma kontrol�
   bool hasConflict = false;
            foreach (var newSchedule in sectionSchedules)
         {
           foreach (var existingSchedule in studentScheduleList)
  {
     if (newSchedule.DayOfWeek == existingSchedule.DayOfWeek &&
    newSchedule.StartTime < existingSchedule.EndTime &&
 newSchedule.EndTime > existingSchedule.StartTime)
  {
     hasConflict = true;
      break;
        }
                }
      if (hasConflict) break;
            }

            if (!hasConflict)
            {
     _logger.LogInformation($"Section {sectionCode} is AVAILABLE");
      return new BestSectionResult
          {
SectionCode = sectionCode,
    EnrolledCount = enrolledCount,
           MaxCapacity = maxCapacity,
      Sessions = sectionSchedules.Select(s => new SessionInfo
  {
         Day = s.DayOfWeek.ToString(),
  StartTime = s.StartTime.ToString(@"hh\:mm"),
      EndTime = s.EndTime.ToString(@"hh\:mm"),
        Room = s.RoomNumber
        }).ToList(),
        Instructor = sectionSchedules.First().InstructorName
       };
            }
        }

        return null;
    }

    private class BestSectionResult
    {
        public string SectionCode { get; set; } = "";
        public int EnrolledCount { get; set; }
        public int MaxCapacity { get; set; }
        public List<SessionInfo> Sessions { get; set; } = new();
    public string? Instructor { get; set; }
    }

    private class SessionInfo
    {
        public string Day { get; set; } = "";
  public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string? Room { get; set; }
    }

// ��rencinin ders program�n� getir
    [HttpGet("my-schedule")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySchedule()
    {
        try
        {
  var userId = GetUserId();

            var enrolledSections = await _db.StudentCourseSections
          .Where(scs => scs.StudentId == userId)
       .Include(scs => scs.Course)
   .ThenInclude(c => c.Category)
    .ToListAsync();

        var scheduleDetails = new List<object>();

       foreach (var section in enrolledSections)
 {
        var schedules = await _db.CourseSchedules
     .Where(cs => cs.CourseId == section.CourseId && cs.SectionCode == section.SectionCode)
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
         semester = section.Course.Semester,
          isCompleted = section.IsCompleted,
      sessions = schedules.Select(s => new
        {
    scheduleId = s.Id,
                dayOfWeek = s.DayOfWeek.ToString(),
      dayOfWeekNumber = (int)s.DayOfWeek,
        dayName = s.DayOfWeek switch
      {
            DayOfWeek.Monday => "Pazartesi",
        DayOfWeek.Tuesday => "Sali",
 DayOfWeek.Wednesday => "Carsamba",
      DayOfWeek.Thursday => "Persembe",
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
                 durationMinutes = (int)(s.EndTime - s.StartTime).TotalMinutes
       }).ToList()
                    });
     }
  }

   // Haftal�k program
            var weeklySchedule = new Dictionary<string, List<object>>
  {
{ "Pazartesi", new List<object>() },
     { "Sali", new List<object>() },
    { "Carsamba", new List<object>() },
    { "Persembe", new List<object>() },
       { "Cuma", new List<object>() }
       };

     foreach (var section in enrolledSections)
    {
         var schedules = await _db.CourseSchedules
         .Where(cs => cs.CourseId == section.CourseId && cs.SectionCode == section.SectionCode)
   .ToListAsync();

     foreach (var schedule in schedules)
         {
               var dayName = schedule.DayOfWeek switch
       {
        DayOfWeek.Monday => "Pazartesi",
  DayOfWeek.Tuesday => "Sali",
             DayOfWeek.Wednesday => "Carsamba",
          DayOfWeek.Thursday => "Persembe",
   DayOfWeek.Friday => "Cuma",
         _ => null
               };

        if (dayName != null && weeklySchedule.ContainsKey(dayName))
        {
        weeklySchedule[dayName].Add(new
      {
              courseCode = section.Course.CourseCode,
    courseName = section.Course.CourseName,
      sectionCode = section.SectionCode,
   startTime = schedule.StartTime.ToString(@"hh\:mm"),
      endTime = schedule.EndTime.ToString(@"hh\:mm"),
     timeSlot = $"{schedule.StartTime:hh\\:mm}-{schedule.EndTime:hh\\:mm}",
    roomNumber = schedule.RoomNumber,
    instructorName = schedule.InstructorName,
            sessionType = schedule.IsTheory ? "Teori" : "Uygulama"
             });
                 }
   }
 }

            // G�nleri saate g�re s�rala
      foreach (var day in weeklySchedule.Keys.ToList())
       {
                weeklySchedule[day] = weeklySchedule[day]
     .OrderBy(x => ((dynamic)x).startTime)
        .ToList();
            }

          var totalCredits = enrolledSections.Sum(s => s.Course.Credits);
       var totalECTS = enrolledSections.Sum(s => s.Course.ECTS);

return Ok(new
 {
         totalCourses = enrolledSections.Count,
    completedCourses = enrolledSections.Count(s => s.IsCompleted),
       totalCredits,
         totalECTS,
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

    // Ders tamamla
  [HttpPatch("{enrollmentId}/complete")]
  [Authorize(Roles = "Student")]
  public async Task<IActionResult> CompleteCourse(int enrollmentId, [FromBody] CompleteCourseDto dto)
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

 return Ok(new { message = "Course completed successfully" });
 }
      catch (Exception ex)
{
    _logger.LogError(ex, "Failed to complete course");
            return StatusCode(500, new { error = "Failed to complete course", details = ex.Message });
    }
    }

    // Dersten ��k
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

          // StudentCourseSection'dan da sil
      var section = await _db.StudentCourseSections
      .FirstOrDefaultAsync(scs => scs.StudentId == userId && scs.CourseId == enrollment.CourseId);

   if (section != null)
        _db.StudentCourseSections.Remove(section);

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

    public record EnrollCourseDto(int CourseId);
    public record CompleteCourseDto(double? Grade, string? LetterGrade, DateTime? CompletionDate);
}
