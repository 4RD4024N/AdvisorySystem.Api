using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/course-selection")]
[Authorize]
[EnableRateLimiting("standard")]
public class CourseSelectionController : ControllerBase
{
    private readonly AppDbContext _db;
  private readonly ILogger<CourseSelectionController> _logger;

    public CourseSelectionController(AppDbContext db, ILogger<CourseSelectionController> logger)
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

  /// <summary>
    /// Hedef öðrenci ID'sini belirler.
    /// Danýþman, kendi öðrencileri için studentId gönderebilir.
    /// Öðrenci ise her zaman kendi ID'sini kullanýr.
    /// </summary>
    private async Task<(string? studentId, IActionResult? error)> ResolveStudentIdAsync(string? requestedStudentId)
    {
      var requesterId = GetUserId();

        if (string.IsNullOrEmpty(requestedStudentId))
       return (requesterId, null);

        if (!User.IsInRole("Advisor"))
       return (null, Forbid());

        var student = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == requestedStudentId && u.AdvisorId == requesterId);

if (student == null)
      return (null, NotFound(new { error = "Öðrenci bulunamadý veya bu danýþmana atanmamýþ." }));

        return (requestedStudentId, null);
    }

    /// <summary>
    /// Tüm dersleri schedule bilgisiyle getir.
/// Danýþman için: ?studentId=... ile öðrenci bazýnda sorgula.
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableCoursesForSelection([FromQuery] string? studentId = null)
    {
        try
        {
  var (targetStudentId, error) = await ResolveStudentIdAsync(studentId);
      if (error != null) return error;

    var enrolledCourseIds = await _db.StudentCourseSections
      .Where(scs => scs.StudentId == targetStudentId)
                .Select(scs => scs.CourseId)
  .ToListAsync();

            var availableCourses = await _db.CourseSchedules
     .Include(cs => cs.Course)
                .ThenInclude(c => c.Category)
          .OrderBy(cs => cs.Course.CourseCode)
      .ThenBy(cs => cs.SectionCode)
   .ToListAsync();

            var groupedCourses = availableCourses
       .GroupBy(cs => new { cs.CourseId, cs.SectionCode })
          .Select(g =>
          {
         var firstSchedule = g.First();
                  var course = firstSchedule.Course;
              var isEnrolled = enrolledCourseIds.Contains(course.Id);
      var enrolledCount = _db.StudentCourseSections.Count(scs =>
   scs.CourseId == course.Id &&
  scs.SectionCode == firstSchedule.SectionCode);

  return new
    {
  courseId = course.Id,
   courseCode = course.CourseCode,
     courseName = course.CourseName,
     description = course.Description,
            credits = course.Credits,
        ects = course.ECTS,
             theoryHours = course.TheoryHours,
    practiceHours = course.PracticeHours,
  isElective = course.IsElective,
       semester = course.Semester,
  category = new
       {
       id = course.Category.Id,
    name = course.Category.Name
              },
     sectionCode = firstSchedule.SectionCode,
           instructor = firstSchedule.InstructorName,
      maxCapacity = firstSchedule.MaxCapacity,
       enrolledCount,
      availableSeats = firstSchedule.MaxCapacity - enrolledCount,
          isFull = enrolledCount >= firstSchedule.MaxCapacity,
     isEnrolled,
  schedule = g.OrderBy(s => s.DayOfWeek)
           .ThenBy(s => s.StartTime)
        .Select(s => new
            {
 dayOfWeek = s.DayOfWeek.ToString(),
   dayOfWeekNumber = (int)s.DayOfWeek,
          startTime = s.StartTime.ToString(@"hh\:mm"),
   endTime = s.EndTime.ToString(@"hh\:mm"),
    roomNumber = s.RoomNumber,
     isTheory = s.IsTheory,
     sessionType = s.IsTheory ? "Teori" : "Uygulama",
 timeSlot = $"{s.DayOfWeek} {s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}"
                })
            .ToList()
     };
                })
  .OrderBy(c => c.courseCode)
     .ThenBy(c => c.sectionCode)
           .ToList();

     return Ok(new
       {
          totalCourses = groupedCourses.Count,
        courses = groupedCourses
    });
        }
     catch (Exception ex)
        {
       _logger.LogError(ex, "Failed to get available courses");
       return StatusCode(500, new { error = "Failed to retrieve courses", details = ex.Message });
      }
    }

    /// <summary>
    /// Derse Kayýt.
    /// Danýþman: dto.StudentId göndererek baþka bir öðrenci adýna kayýt yapabilir.
    /// </summary>
 [HttpPost("enroll")]
    public async Task<IActionResult> EnrollInCourse([FromBody] EnrollWithScheduleDto dto)
    {
        try
        {
         var (targetStudentId, resolveError) = await ResolveStudentIdAsync(dto.StudentId);
   if (resolveError != null) return resolveError;

          var schedule = await _db.CourseSchedules
            .Include(cs => cs.Course)
     .FirstOrDefaultAsync(cs =>
        cs.CourseId == dto.CourseId &&
          cs.SectionCode == dto.SectionCode);

            if (schedule == null)
          return NotFound(new { error = "Course schedule not found" });

            var alreadyEnrolled = await _db.StudentCourseSections
                .AnyAsync(scs => scs.StudentId == targetStudentId && scs.CourseId == dto.CourseId);

     if (alreadyEnrolled)
       return BadRequest(new { error = "Already enrolled in this course" });

         var enrolledCount = await _db.StudentCourseSections
         .CountAsync(scs => scs.CourseId == dto.CourseId && scs.SectionCode == dto.SectionCode);

    if (enrolledCount >= schedule.MaxCapacity)
       return BadRequest(new { error = "Course section is full" });

            var hasConflict = await CheckScheduleConflict(targetStudentId!, schedule);
          if (hasConflict.IsConflict)
   return BadRequest(new
              {
 error = "Schedule conflict",
     message = hasConflict.Message,
      conflictingCourse = hasConflict.ConflictingCourse
      });

            await using var transaction = await _db.Database.BeginTransactionAsync();
  try
       {
     var studentCourse = new StudentCourse
      {
StudentId = targetStudentId!,
          CourseId = dto.CourseId,
        Semester = schedule.Course.Semester,
      IsCompleted = false
 };
          _db.StudentCourses.Add(studentCourse);

     var section = new StudentCourseSection
      {
         StudentId = targetStudentId!,
          CourseId = dto.CourseId,
      SectionCode = dto.SectionCode,
   IsCompleted = false
            };
  _db.StudentCourseSections.Add(section);

     await _db.SaveChangesAsync();
 await transaction.CommitAsync();

          var addedSchedules = await _db.CourseSchedules
            .Where(cs => cs.CourseId == dto.CourseId && cs.SectionCode == dto.SectionCode)
    .Select(s => new
       {
                dayOfWeek = s.DayOfWeek.ToString(),
       startTime = s.StartTime.ToString(@"hh\:mm"),
       endTime = s.EndTime.ToString(@"hh\:mm"),
                roomNumber = s.RoomNumber,
    isTheory = s.IsTheory
     })
       .ToListAsync();

    return Ok(new
    {
        message = "Enrolled successfully",
            enrollmentId = studentCourse.Id,
         courseCode = schedule.Course.CourseCode,
      courseName = schedule.Course.CourseName,
         sectionCode = dto.SectionCode,
     schedule = addedSchedules
    });
     }
       catch
    {
     await transaction.RollbackAsync();
   throw;
            }
    }
    catch (Exception ex)
    {
            _logger.LogError(ex, "Failed to enroll in course");
    return StatusCode(500, new { error = "Failed to enroll", details = ex.Message });
}
    }

    /// <summary>
    /// Dersten Çýk.
    /// Danýþman: ?studentId=... ile öðrenci adýna iþlem yapabilir.
    /// </summary>
    [HttpDelete("unenroll/{courseId}")]
    public async Task<IActionResult> UnenrollFromCourse(int courseId, [FromQuery] string? studentId = null)
    {
        try
   {
      var (targetStudentId, error) = await ResolveStudentIdAsync(studentId);
            if (error != null) return error;

       var section = await _db.StudentCourseSections
   .FirstOrDefaultAsync(scs => scs.StudentId == targetStudentId && scs.CourseId == courseId);

       if (section == null)
   return NotFound(new { error = "Enrollment not found" });

      if (section.IsCompleted)
    return BadRequest(new { error = "Cannot unenroll from completed course" });

            var studentCourse = await _db.StudentCourses
    .FirstOrDefaultAsync(sc => sc.StudentId == targetStudentId && sc.CourseId == courseId);

            if (studentCourse != null)
      _db.StudentCourses.Remove(studentCourse);

          _db.StudentCourseSections.Remove(section);
  await _db.SaveChangesAsync();

return Ok(new { message = "Unenrolled successfully" });
        }
        catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to unenroll");
   return StatusCode(500, new { error = "Failed to unenroll", details = ex.Message });
     }
    }

    private async Task<(bool IsConflict, string Message, object? ConflictingCourse)> CheckScheduleConflict(
        string userId,
   CourseSchedule newSchedule)
    {
    var existingSections = await _db.StudentCourseSections
.Where(scs => scs.StudentId == userId)
     .Select(scs => new { scs.CourseId, scs.SectionCode })
    .ToListAsync();

        foreach (var existing in existingSections)
        {
            var existingSchedules = await _db.CourseSchedules
    .Include(cs => cs.Course)
                .Where(cs => cs.CourseId == existing.CourseId && cs.SectionCode == existing.SectionCode)
   .ToListAsync();

        var newSchedules = await _db.CourseSchedules
.Where(cs => cs.CourseId == newSchedule.CourseId && cs.SectionCode == newSchedule.SectionCode)
       .ToListAsync();

       foreach (var newSch in newSchedules)
         {
    foreach (var existingSch in existingSchedules)
     {
           if (newSch.DayOfWeek == existingSch.DayOfWeek &&
                 !(newSch.EndTime <= existingSch.StartTime || newSch.StartTime >= existingSch.EndTime))
        {
    return (true,
    $"Schedule conflict on {newSch.DayOfWeek} with {existingSch.Course.CourseCode}",
              new
   {
 courseCode = existingSch.Course.CourseCode,
              courseName = existingSch.Course.CourseName,
       day = existingSch.DayOfWeek.ToString(),
           time = $"{existingSch.StartTime:hh\\:mm}-{existingSch.EndTime:hh\\:mm}"
           });
                    }
       }
      }
}

        return (false, string.Empty, null);
    }

    public record EnrollWithScheduleDto(int CourseId, string SectionCode, string? StudentId = null);
}
