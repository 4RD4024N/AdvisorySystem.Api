using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/section-enrollment")]
[Authorize]
[EnableRateLimiting("standard")]
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

    // Öðrencinin mevcut kayýtlý derslerini getir
[HttpGet("my-enrollments")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        try
{
  var userId = GetUserId();

            var enrollments = await _db.StudentCourseSections
          .Where(scs => scs.StudentId == userId)
                .Include(scs => scs.Course)
  .ThenInclude(c => c.Category)
   .ToListAsync();

          var result = new List<object>();
      foreach (var enrollment in enrollments)
            {
       var schedules = await _db.CourseSchedules
       .Where(cs => cs.CourseId == enrollment.CourseId && cs.SectionCode == enrollment.SectionCode)
     .OrderBy(cs => cs.DayOfWeek)
  .ThenBy(cs => cs.StartTime)
    .ToListAsync();

    result.Add(new
    {
       enrollmentId = enrollment.Id,
      courseId = enrollment.CourseId,
  courseCode = enrollment.Course.CourseCode,
     courseName = enrollment.Course.CourseName,
                description = enrollment.Course.Description,
         credits = enrollment.Course.Credits,
   ects = enrollment.Course.ECTS,
     category = enrollment.Course.Category.Name,
  sectionCode = enrollment.SectionCode,
    isCompleted = enrollment.IsCompleted,
   enrolledAt = enrollment.EnrolledAt,
        schedule = schedules.Select(s => new
       {
          dayOfWeek = s.DayOfWeek.ToString(),
              startTime = s.StartTime.ToString(@"hh\:mm"),
  endTime = s.EndTime.ToString(@"hh\:mm"),
        roomNumber = s.RoomNumber,
        instructorName = s.InstructorName,
 isTheory = s.IsTheory
 }).ToList()
      });
      }

         return Ok(new
            {
      totalCourses = enrollments.Count,
       totalCredits = enrollments.Sum(e => e.Course.Credits),
  totalECTS = enrollments.Sum(e => e.Course.ECTS),
           courses = result
     });
        }
  catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to get enrollments");
            return StatusCode(500, new { error = "Failed to retrieve enrollments", details = ex.Message });
    }
    }

    // Derse kayýt ol
    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollInCourse([FromBody] EnrollDto dto)
    {
        try
        {
var userId = GetUserId();

       _logger.LogInformation($"Enrollment request: userId={userId}, courseId={dto.CourseId}, section={dto.SectionCode}");

      // Ders var mý kontrol et
            var course = await _db.Courses.FindAsync(dto.CourseId);
            if (course == null)
       return NotFound(new { error = "Course not found" });

 // Bu ders için schedule var mý kontrol et
    var courseSchedules = await _db.CourseSchedules
      .Where(cs => cs.CourseId == dto.CourseId)
    .ToListAsync();

            if (!courseSchedules.Any())
        {
     _logger.LogWarning($"Course {dto.CourseId} has no schedule");
            return BadRequest(new
    {
     error = "Course schedule not found",
   message = $"This course ({course.CourseCode}) doesn't have a schedule. Please contact admin to generate schedules first."
      });
        }

            // Zaten kayýtlý mý kontrol et
       var existingEnrollment = await _db.StudentCourseSections
         .FirstOrDefaultAsync(scs => scs.StudentId == userId && scs.CourseId == dto.CourseId);

       if (existingEnrollment != null)
                return BadRequest(new { error = "Already enrolled in this course" });

        // Section belirle
        string sectionCode;
    if (string.IsNullOrEmpty(dto.SectionCode) || dto.SectionCode == "TBD")
     {
                var bestSection = await FindBestAvailableSectionAsync(userId, dto.CourseId);
           if (bestSection == null)
   {
     return BadRequest(new
     {
      error = "No available section",
   message = "All sections are either full or conflict with your existing schedule. Please contact your advisor."
    });
   }
        sectionCode = bestSection.SectionCode;
     _logger.LogInformation($"Auto-selected section {sectionCode} for course {dto.CourseId}");
            }
    else
        {
          sectionCode = dto.SectionCode;

   // Section var mý kontrol et
var sectionExists = courseSchedules.Any(cs => cs.SectionCode == sectionCode);
  if (!sectionExists)
          {
         return BadRequest(new
     {
                   error = "Section not found",
    message = $"Section {sectionCode} doesn't exist. Available sections: " +
   string.Join(", ", courseSchedules.Select(cs => cs.SectionCode).Distinct())
         });
    }

              // Çakýþma kontrolü
           var conflict = await CheckScheduleConflictAsync(userId, dto.CourseId, sectionCode);
       if (conflict.hasConflict)
                {
               _logger.LogWarning($"Schedule conflict detected for section {sectionCode}");
       return BadRequest(new
      {
         error = "Schedule conflict detected",
             message = "This course overlaps with your existing schedule",
        conflictDetails = conflict.conflictingCourses
            });
    }

  // Kapasite kontrolü
           var sectionSchedule = courseSchedules.First(cs => cs.SectionCode == sectionCode);
        var enrolledCount = await _db.StudentCourseSections
          .CountAsync(scs => scs.CourseId == dto.CourseId && scs.SectionCode == sectionCode);

             if (enrolledCount >= sectionSchedule.MaxCapacity)
{
           return BadRequest(new
          {
    error = "Section is full",
     message = $"Section {sectionCode} has reached maximum capacity",
      enrolledCount,
 maxCapacity = sectionSchedule.MaxCapacity
   });
         }
        }

   var enrollment = new StudentCourseSection
   {
       StudentId = userId,
    CourseId = dto.CourseId,
 SectionCode = sectionCode,
          IsCompleted = false
    };

  _db.StudentCourseSections.Add(enrollment);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Successfully enrolled student {userId} in course {dto.CourseId}, section {sectionCode}");

            return Ok(new
      {
  message = "Enrolled successfully",
   enrollmentId = enrollment.Id,
    sectionCode = enrollment.SectionCode,
   courseCode = course.CourseCode,
  courseName = course.CourseName
          });
        }
        catch (Exception ex)
        {
         _logger.LogError(ex, "Failed to enroll");
 return StatusCode(500, new { error = "Failed to enroll", details = ex.Message });
        }
    }

    // Dersten çýk
 [HttpDelete("unenroll/{courseId}")]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        try
        {
  var userId = GetUserId();

            var enrollment = await _db.StudentCourseSections
   .FirstOrDefaultAsync(scs => scs.StudentId == userId && scs.CourseId == courseId);

            if (enrollment == null)
    return NotFound(new { error = "Enrollment not found" });

            if (enrollment.IsCompleted)
      return BadRequest(new { error = "Cannot unenroll from completed course" });

            _db.StudentCourseSections.Remove(enrollment);
       await _db.SaveChangesAsync();

         return Ok(new { message = "Unenrolled successfully" });
      }
      catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to unenroll");
   return StatusCode(500, new { error = "Failed to unenroll", details = ex.Message });
      }
    }

    // En uygun section'ý bul
    private async Task<SectionResult?> FindBestAvailableSectionAsync(string studentId, int courseId)
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

        // Öðrencinin mevcut schedule'larýný al
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

            // Kapasite kontrolü
            var enrolledCount = await _db.StudentCourseSections
     .CountAsync(scs => scs.CourseId == courseId && scs.SectionCode == sectionCode);

  var maxCapacity = sectionSchedules.First().MaxCapacity;

            if (enrolledCount >= maxCapacity)
            {
         _logger.LogInformation($"Section {sectionCode} is FULL ({enrolledCount}/{maxCapacity})");
         continue;
  }

     // Çakýþma kontrolü
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
     return new SectionResult { SectionCode = sectionCode };
      }
      }

  return null;
    }

    // Çakýþma kontrolü
    private async Task<(bool hasConflict, List<object> conflictingCourses)> CheckScheduleConflictAsync(
        string studentId,
     int newCourseId,
        string newSectionCode)
    {
        var conflictingCourses = new List<object>();

      var newCourseSchedules = await _db.CourseSchedules
            .Where(cs => cs.CourseId == newCourseId && cs.SectionCode == newSectionCode)
            .ToListAsync();

    if (!newCourseSchedules.Any())
        {
            conflictingCourses.Add(new
            {
   courseCode = "UNKNOWN",
        courseName = "Schedule not found",
          sectionCode = newSectionCode,
     day = "N/A"
    });
            return (true, conflictingCourses);
}

      var studentEnrollments = await _db.StudentCourseSections
            .Where(scs => scs.StudentId == studentId && !scs.IsCompleted)
 .Include(scs => scs.Course)
            .ToListAsync();

    foreach (var enrollment in studentEnrollments)
        {
         var existingSchedules = await _db.CourseSchedules
        .Where(cs => cs.CourseId == enrollment.CourseId && cs.SectionCode == enrollment.SectionCode)
         .ToListAsync();

    foreach (var newSchedule in newCourseSchedules)
            {
foreach (var existingSchedule in existingSchedules)
      {
        if (newSchedule.DayOfWeek == existingSchedule.DayOfWeek)
        {
    bool hasOverlap = newSchedule.StartTime < existingSchedule.EndTime &&
        newSchedule.EndTime > existingSchedule.StartTime;

            if (hasOverlap)
 {
       conflictingCourses.Add(new
  {
     courseCode = enrollment.Course.CourseCode,
      courseName = enrollment.Course.CourseName,
      sectionCode = enrollment.SectionCode,
  day = existingSchedule.DayOfWeek.ToString(),
         existingTime = $"{existingSchedule.StartTime:hh\\:mm} - {existingSchedule.EndTime:hh\\:mm}",
        newTime = $"{newSchedule.StartTime:hh\\:mm} - {newSchedule.EndTime:hh\\:mm}"
     });
  }
              }
      }
    }
        }

      return (conflictingCourses.Any(), conflictingCourses);
    }

    private class SectionResult
    {
        public string SectionCode { get; set; } = "";
    }

    // Semester kaldýrýldý - öðrencinin tek bir programý var
public record EnrollDto(int CourseId, string? SectionCode);
}
