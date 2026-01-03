using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/course-selection")]
[Authorize]
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
    /// Öðrenci Ders Seçimi Ýçin: Tüm dersleri schedule bilgisiyle getir
  /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableCoursesForSelection()
    {
     try
        {
        var userId = GetUserId();

   // Öðrencinin zaten kayýtlý olduðu dersleri bul
     var enrolledCourseIds = await _db.StudentCourseSections
      .Where(scs => scs.StudentId == userId)
         .Select(scs => scs.CourseId)
 .ToListAsync();

          // Tüm course schedule'larý al
    var availableCourses = await _db.CourseSchedules
     .Include(cs => cs.Course)
     .ThenInclude(c => c.Category)
.OrderBy(cs => cs.Course.CourseCode)
    .ThenBy(cs => cs.SectionCode)
  .ToListAsync();

 // Dersleri grupla (CourseId + SectionCode bazýnda)
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
    /// Öðrenci Derse Kayýt
    /// </summary>
    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollInCourse([FromBody] EnrollWithScheduleDto dto)
    {
     try
        {
            var userId = GetUserId();

      // Dersin schedule bilgisini al
   var schedule = await _db.CourseSchedules
       .Include(cs => cs.Course)
  .FirstOrDefaultAsync(cs => 
       cs.CourseId == dto.CourseId && 
          cs.SectionCode == dto.SectionCode);

       if (schedule == null)
       return NotFound(new { error = "Course schedule not found" });

     // Zaten kayýtlý mý kontrol et
  var alreadyEnrolled = await _db.StudentCourseSections
   .AnyAsync(scs => scs.StudentId == userId && scs.CourseId == dto.CourseId);

  if (alreadyEnrolled)
    return BadRequest(new { error = "Already enrolled in this course" });

    // Kapasite kontrolü
       var enrolledCount = await _db.StudentCourseSections
       .CountAsync(scs => scs.CourseId == dto.CourseId && scs.SectionCode == dto.SectionCode);

    if (enrolledCount >= schedule.MaxCapacity)
                return BadRequest(new { error = "Course section is full" });

  // Zaman çakýþmasý kontrolü
 var hasConflict = await CheckScheduleConflict(userId, schedule);
     if (hasConflict.IsConflict)
 return BadRequest(new 
    { 
  error = "Schedule conflict",
message = hasConflict.Message,
      conflictingCourse = hasConflict.ConflictingCourse
 });

            // StudentCourse kaydý
         var studentCourse = new StudentCourse
     {
      StudentId = userId,
       CourseId = dto.CourseId,
      Semester = schedule.Course.Semester,
     IsCompleted = false
  };
          _db.StudentCourses.Add(studentCourse);

            // StudentCourseSection kaydý
    var section = new StudentCourseSection
            {
       StudentId = userId,
         CourseId = dto.CourseId,
        SectionCode = dto.SectionCode,
   IsCompleted = false
       };
   _db.StudentCourseSections.Add(section);

            await _db.SaveChangesAsync();

    // Eklenen dersin schedule bilgilerini döndür
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
        catch (Exception ex)
      {
  _logger.LogError(ex, "Failed to enroll in course");
   return StatusCode(500, new { error = "Failed to enroll", details = ex.Message });
        }
    }

    /// <summary>
    /// Dersten Çýk
    /// </summary>
    [HttpDelete("unenroll/{courseId}")]
  public async Task<IActionResult> UnenrollFromCourse(int courseId)
    {
 try
        {
      var userId = GetUserId();

       var section = await _db.StudentCourseSections
 .FirstOrDefaultAsync(scs => scs.StudentId == userId && scs.CourseId == courseId);

      if (section == null)
   return NotFound(new { error = "Enrollment not found" });

  if (section.IsCompleted)
             return BadRequest(new { error = "Cannot unenroll from completed course" });

            var studentCourse = await _db.StudentCourses
   .FirstOrDefaultAsync(sc => sc.StudentId == userId && sc.CourseId == courseId);

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

    /// <summary>
    /// Zaman çakýþmasý kontrolü
    /// </summary>
    private async Task<(bool IsConflict, string Message, object? ConflictingCourse)> CheckScheduleConflict(
    string userId, 
        CourseSchedule newSchedule)
    {
        // Öðrencinin mevcut derslerini al
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
        if (newSch.DayOfWeek == existingSch.DayOfWeek)
        {
          if (!(newSch.EndTime <= existingSch.StartTime || newSch.StartTime >= existingSch.EndTime))
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
    }

        return (false, string.Empty, null);
    }

    public record EnrollWithScheduleDto(int CourseId, string SectionCode);
}
