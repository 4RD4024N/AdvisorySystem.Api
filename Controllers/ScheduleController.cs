using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/schedule")]
[Authorize]
[EnableRateLimiting("standard")]
public class ScheduleController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICourseScheduler _scheduler;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(AppDbContext db, ICourseScheduler scheduler, ILogger<ScheduleController> logger)
    {
        _db = db;
        _scheduler = scheduler;
        _logger = logger;
    }

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<IActionResult> SearchCourses([FromQuery] string? query = null, [FromQuery] int? semester = null)
    {
        try
        {
          var coursesQuery = _db.CourseSchedules.Include(cs => cs.Course).ThenInclude(c => c.Category).AsQueryable();

    if (semester.HasValue)
       coursesQuery = coursesQuery.Where(cs => cs.Semester == semester.Value);

            if (!string.IsNullOrWhiteSpace(query))
   {
           query = query.ToLower().Trim();
                coursesQuery = coursesQuery.Where(cs =>
           cs.Course.CourseName.ToLower().Contains(query) ||
         cs.Course.CourseCode.ToLower().Contains(query) ||
       cs.Course.Category.Name.ToLower().Contains(query) ||
       (cs.Course.Description != null && cs.Course.Description.ToLower().Contains(query)));
            }

    var results = await coursesQuery.OrderBy(cs => cs.Course.CourseCode).Select(cs => new
  {
                scheduleId = cs.Id,
                courseId = cs.CourseId,
      courseCode = cs.Course.CourseCode,
      courseName = cs.Course.CourseName,
     description = cs.Course.Description,
                category = cs.Course.Category.Name,
         semester = cs.Semester,
          sectionCode = cs.SectionCode,
 credits = cs.Course.Credits,
     ects = cs.Course.ECTS,
   theoryHours = cs.Course.TheoryHours,
     practiceHours = cs.Course.PracticeHours,
           dayOfWeek = cs.DayOfWeek.ToString(),
            startTime = cs.StartTime.ToString(@"hh\:mm"),
   endTime = cs.EndTime.ToString(@"hh\:mm"),
          isTheory = cs.IsTheory,
             roomNumber = cs.RoomNumber,
  instructorName = cs.InstructorName,
           maxCapacity = cs.MaxCapacity,
   enrolledCount = _db.StudentCourseSections.Count(scs => scs.CourseId == cs.CourseId && scs.SectionCode == cs.SectionCode)
            }).ToListAsync();

    return Ok(new { query = query ?? "all", semester, totalResults = results.Count, courses = results });
        }
     catch (Exception ex)
        {
         _logger.LogError(ex, "Failed to search courses");
            return StatusCode(500, new { error = "Failed to search courses", details = ex.Message });
      }
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableCourses()
 {
        try
        {
         var schedules = await _db.CourseSchedules
        .Include(cs => cs.Course)
             .ThenInclude(c => c.Category)
      .ToListAsync();

            // Dersleri grupla (courseId + sectionCode bazýnda - semester yok)
        var groupedCourses = schedules
   .GroupBy(s => new { s.CourseId, s.SectionCode })
          .Select(g =>
          {
         var first = g.First();
                  var enrolledCount = _db.StudentCourseSections
       .Count(scs => scs.CourseId == first.CourseId && scs.SectionCode == first.SectionCode);

    return new
 {
      courseId = first.CourseId,
        courseCode = first.Course.CourseCode,
         courseName = first.Course.CourseName,
     description = first.Course.Description,
     category = first.Course.Category.Name,
         semester = first.Course.Semester, // Dersin ait olduðu dönem (Course tablosundan)
       sectionCode = first.SectionCode,
    credits = first.Course.Credits,
        ects = first.Course.ECTS,
           theoryHours = first.Course.TheoryHours,
  practiceHours = first.Course.PracticeHours,
       isElective = first.Course.IsElective,
instructor = first.InstructorName,
                  maxCapacity = first.MaxCapacity,
     enrolledCount,
               availableSeats = first.MaxCapacity - enrolledCount,
        isFull = enrolledCount >= first.MaxCapacity,
     schedule = g.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).Select(s => new
   {
            scheduleId = s.Id,
  dayOfWeek = s.DayOfWeek.ToString(),
   startTime = s.StartTime.ToString(@"hh\:mm"),
  endTime = s.EndTime.ToString(@"hh\:mm"),
                roomNumber = s.RoomNumber,
          sessionType = s.IsTheory ? "Teori" : "Uygulama"
      }).ToList()
  };
        })
      .OrderBy(c => c.semester)
       .ThenBy(c => c.courseCode)
        .ThenBy(c => c.sectionCode)
           .ToList();

      var requiredCourses = groupedCourses.Where(c => !c.isElective).ToList();
            var electiveCourses = groupedCourses.Where(c => c.isElective).ToList();

      return Ok(new
       {
           totalCourses = groupedCourses.Count,
  requiredCount = requiredCourses.Count,
       electiveCount = electiveCourses.Count,
        availableCourses = groupedCourses.Count(c => !c.isFull),
       fullCourses = groupedCourses.Count(c => c.isFull),
        courses = groupedCourses
         });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available courses");
       return StatusCode(500, new { error = "Failed to retrieve available courses", details = ex.Message });
        }
    }

    [HttpPost("generate/{semester}")]
    [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GenerateSchedule(int semester)
    {
      try
     {
   _logger.LogInformation($"Generating schedule for semester {semester}");
 var schedules = await _scheduler.GenerateScheduleForSemesterAsync(semester);
            var conflicts = await _scheduler.DetectConflictsAsync(semester);

            return Ok(new
       {
     message = $"Schedule generated for semester {semester}",
     totalSchedules = schedules.Count,
         conflicts = conflicts.Count,
    schedule = schedules.Select(s => new
      {
        s.Id,
        courseId = s.CourseId,
        courseCode = s.Course?.CourseCode,
 courseName = s.Course?.CourseName,
     dayOfWeek = s.DayOfWeek.ToString(),
        startTime = s.StartTime.ToString(@"hh\:mm"),
                  endTime = s.EndTime.ToString(@"hh\:mm"),
        s.IsTheory,
                    s.RoomNumber,
         s.InstructorName
    }).OrderBy(s => s.dayOfWeek).ThenBy(s => s.startTime)
     });
        }
  catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to generate schedule");
    return StatusCode(500, new { error = "Failed to generate schedule", details = ex.Message });
        }
    }

[HttpGet("semester/{semester}")]
  public async Task<IActionResult> GetScheduleBySemester(int semester)
    {
        try
        {
    var schedules = await _db.CourseSchedules.Include(cs => cs.Course).ThenInclude(c => c.Category)
        .Where(cs => cs.Semester == semester).OrderBy(cs => cs.DayOfWeek).ThenBy(cs => cs.StartTime).Select(cs => new
      {
  cs.Id,
     cs.CourseId,
  courseCode = cs.Course.CourseCode,
         courseName = cs.Course.CourseName,
              theoryHours = cs.Course.TheoryHours,
            practiceHours = cs.Course.PracticeHours,
         credits = cs.Course.Credits,
             ects = cs.Course.ECTS,
       dayOfWeek = cs.DayOfWeek.ToString(),
 startTime = cs.StartTime.ToString(@"hh\:mm"),
               endTime = cs.EndTime.ToString(@"hh\:mm"),
        durationMinutes = (int)(cs.EndTime - cs.StartTime).TotalMinutes,
              cs.IsTheory,
   cs.RoomNumber,
          cs.InstructorName,
       category = cs.Course.Category.Name
            }).ToListAsync();

var byDay = schedules.GroupBy(s => s.dayOfWeek).Select(g => new { day = g.Key, courses = g.ToList() }).ToList();
         return Ok(new { semester, totalSchedules = schedules.Count, byDay, allSchedules = schedules });
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to get schedule");
         return StatusCode(500, new { error = "Failed to retrieve schedule", details = ex.Message });
        }
    }

    [HttpGet("week/{semester}")]
    public async Task<IActionResult> GetWeeklySchedule(int semester)
    {
  try
        {
            var schedules = await _db.CourseSchedules.Include(cs => cs.Course).Where(cs => cs.Semester == semester).ToListAsync();
    var weeklyGrid = new Dictionary<DayOfWeek, List<object>>();

    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
      {
          if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) continue;

                var daySchedules = schedules.Where(s => s.DayOfWeek == day).OrderBy(s => s.StartTime).Select(s => new
   {
        s.Id,
     courseCode = s.Course.CourseCode,
             courseName = s.Course.CourseName,
      startTime = s.StartTime.ToString(@"hh\:mm"),
   endTime = s.EndTime.ToString(@"hh\:mm"),
              duration = $"{(int)(s.EndTime - s.StartTime).TotalMinutes} min",
             type = s.IsTheory ? "Theory" : "Practice",
           s.RoomNumber,
          s.InstructorName
                }).ToList<object>();

    weeklyGrid[day] = daySchedules;
     }

        return Ok(new { semester, weeklySchedule = weeklyGrid.Select(kvp => new { day = kvp.Key.ToString(), courses = kvp.Value }) });
        }
catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to get weekly schedule");
        return StatusCode(500, new { error = "Failed to retrieve weekly schedule", details = ex.Message });
        }
    }

    [HttpGet("conflicts/{semester}")]
[Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetConflicts(int semester)
    {
      try
        {
     var conflicts = await _scheduler.DetectConflictsAsync(semester);
         var conflictDetails = new List<object>();

  foreach (var conflict in conflicts)
 {
                var schedule1 = await _db.CourseSchedules.Include(cs => cs.Course).FirstOrDefaultAsync(cs => cs.Id == conflict.Schedule1Id);
    var schedule2 = await _db.CourseSchedules.Include(cs => cs.Course).FirstOrDefaultAsync(cs => cs.Id == conflict.Schedule2Id);

           if (schedule1 != null && schedule2 != null)
        {
       conflictDetails.Add(new
           {
         conflict.ConflictType,
 conflict.Description,
     course1 = new
        {
  schedule1.Course.CourseCode,
         schedule1.Course.CourseName,
         day = schedule1.DayOfWeek.ToString(),
         startTime = schedule1.StartTime.ToString(@"hh\:mm"),
      endTime = schedule1.EndTime.ToString(@"hh\:mm")
             },
       course2 = new
             {
schedule2.Course.CourseCode,
  schedule2.Course.CourseName,
  day = schedule2.DayOfWeek.ToString(),
           startTime = schedule2.StartTime.ToString(@"hh\:mm"),
         endTime = schedule2.EndTime.ToString(@"hh\:mm")
         },
            conflict.DetectedAt
      });
        }
          }

  return Ok(new { semester, totalConflicts = conflictDetails.Count, conflicts = conflictDetails });
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Failed to detect conflicts");
            return StatusCode(500, new { error = "Failed to detect conflicts", details = ex.Message });
      }
    }

    [HttpDelete("semester/{semester}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSchedule(int semester)
    {
    try
        {
     var schedules = await _db.CourseSchedules.Where(cs => cs.Semester == semester).ToListAsync();
 _db.CourseSchedules.RemoveRange(schedules);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Schedule for semester {semester} deleted", deletedCount = schedules.Count });
        }
  catch (Exception ex)
        {
  _logger.LogError(ex, "Failed to delete schedule");
 return StatusCode(500, new { error = "Failed to delete schedule", details = ex.Message });
        }
    }

    [HttpPut("{scheduleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] UpdateScheduleDto dto)
    {
   try
      {
            var schedule = await _db.CourseSchedules.FindAsync(scheduleId);
       if (schedule == null) return NotFound(new { error = "Schedule not found" });

            if (dto.DayOfWeek.HasValue) schedule.DayOfWeek = dto.DayOfWeek.Value;
    if (dto.StartTime.HasValue) schedule.StartTime = dto.StartTime.Value;
      if (dto.EndTime.HasValue) schedule.EndTime = dto.EndTime.Value;
            if (dto.RoomNumber != null) schedule.RoomNumber = dto.RoomNumber;
    if (dto.InstructorName != null) schedule.InstructorName = dto.InstructorName;

   var hasConflict = await _scheduler.HasConflictAsync(schedule);
       if (hasConflict) return BadRequest(new { error = "Schedule update would create a conflict" });

   await _db.SaveChangesAsync();
      return Ok(new { message = "Schedule updated successfully" });
 }
        catch (Exception ex)
      {
   _logger.LogError(ex, "Failed to update schedule");
 return StatusCode(500, new { error = "Failed to update schedule", details = ex.Message });
        }
    }

    public record UpdateScheduleDto(DayOfWeek? DayOfWeek, TimeSpan? StartTime, TimeSpan? EndTime, string? RoomNumber, string? InstructorName);
}
