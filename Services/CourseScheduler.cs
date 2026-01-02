using AdvisorySystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Services;

public interface ICourseScheduler
{
    Task<List<CourseSchedule>> GenerateScheduleForSemesterAsync(int semester);
    Task<bool> HasConflictAsync(CourseSchedule schedule);
    Task<List<ScheduleConflict>> DetectConflictsAsync(int semester);
}

public class CourseScheduler : ICourseScheduler
{
    private readonly AppDbContext _db;
    private readonly ILogger<CourseScheduler> _logger;

    // Ders saatleri: 09:00 - 17:00 arasý, her ders 50 dakika
private readonly List<TimeSpan> _timeSlots = new()
    {
new TimeSpan(9, 0, 0),   // 09:00
        new TimeSpan(10, 0, 0),  // 10:00
   new TimeSpan(11, 0, 0),  // 11:00
      new TimeSpan(13, 0, 0),  // 13:00 (öðle arasý sonrasý)
        new TimeSpan(14, 0, 0),  // 14:00
  new TimeSpan(15, 0, 0),  // 15:00
   new TimeSpan(16, 0, 0)   // 16:00
    };

    private readonly List<DayOfWeek> _workDays = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    public CourseScheduler(AppDbContext db, ILogger<CourseScheduler> logger)
 {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CourseSchedule>> GenerateScheduleForSemesterAsync(int semester)
    {
   try
        {
   // Clear existing schedules for this semester
 var existingSchedules = await _db.CourseSchedules
 .Where(cs => cs.Semester == semester)
  .ToListAsync();
          _db.CourseSchedules.RemoveRange(existingSchedules);
  await _db.SaveChangesAsync();

            // Get all courses for this semester
         var courses = await _db.Courses
     .Include(c => c.Category)
   .Where(c => c.Semester == semester && !c.IsElective)
   .OrderByDescending(c => c.TotalWeeklyHours)
   .ToListAsync();

  var schedules = new List<CourseSchedule>();
   
 // Create multiple sections (A, B, C) for each course
   var sections = new[] { "A", "B", "C" };
     
            foreach (var sectionCode in sections)
   {
    var usedSlots = new Dictionary<string, bool>();
   
           foreach (var course in courses)
        {
    var courseSchedules = await AssignCourseToSlotsAsync(
       course,
semester,
      sectionCode,
  usedSlots);

      if (courseSchedules.Any())
      {
             schedules.AddRange(courseSchedules);
           _db.CourseSchedules.AddRange(courseSchedules);
         }
 else
    {
    _logger.LogWarning($"Could not schedule course: {course.CourseCode} - Section {sectionCode}");
        }
  }
  }

        await _db.SaveChangesAsync();

            _logger.LogInformation($"Generated {schedules.Count} schedule entries for semester {semester}");
            return schedules;
        }
     catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to generate schedule");
   throw;
   }
    }

    private async Task<List<CourseSchedule>> AssignCourseToSlotsAsync(
Course course,
      int semester,
  string sectionCode,
     Dictionary<string, bool> usedSlots)
    {
   var schedules = new List<CourseSchedule>();
      var totalHours = course.TotalWeeklyHours;

        // Determine session durations
   var sessions = SplitIntoSessions(totalHours);
int sessionNumber = 1;

   foreach (var sessionHours in sessions)
  {
   var assigned = false;

  // Try to find an available slot
   foreach (var day in _workDays)
         {
   if (assigned) break;

           foreach (var startTime in _timeSlots)
    {
      var slotKey = $"{day}_{startTime}";

     // Check if slot is already used
           if (usedSlots.ContainsKey(slotKey))
   continue;

 // Check if we have enough consecutive time
                    if (!HasConsecutiveSlots(day, startTime, sessionHours, usedSlots))
    continue;

   // Assign this slot
     var endTime = startTime.Add(TimeSpan.FromMinutes(sessionHours * 50));

  var schedule = new CourseSchedule
      {
       CourseId = course.Id,
       Semester = semester,
       SectionCode = sectionCode,
    SessionNumber = sessionNumber,
       DayOfWeek = day,
    StartTime = startTime,
   EndTime = endTime,
     IsTheory = sessionNumber <= (sessions.Count - (course.PracticeHours > 0 ? 1 : 0)),
 MaxCapacity = 50,
  CreatedAt = DateTime.UtcNow
  };

    schedules.Add(schedule);

   // Mark slots as used
     for (int i = 0; i < sessionHours; i++)
      {
        var slotTime = startTime.Add(TimeSpan.FromHours(i));
              usedSlots[$"{day}_{slotTime}"] = true;
        }

   assigned = true;
     sessionNumber++;
         break;
           }
   }

    if (!assigned)
      {
     _logger.LogWarning($"Could not assign session {sessionNumber} for {course.CourseCode} - Section {sectionCode}");
       }
     }

    return schedules;
    }

    private List<int> SplitIntoSessions(int totalHours)
    {
        var sessions = new List<int>();

      // 4 saat ? 2 + 2
        // 3 saat ? 2 + 1
        // 2 saat ? 2
 // 1 saat ? 1

  while (totalHours > 0)
        {
      if (totalHours >= 4)
     {
             sessions.Add(2);
           sessions.Add(2);
       totalHours -= 4;
      }
    else if (totalHours >= 3)
        {
              sessions.Add(2);
       sessions.Add(1);
  totalHours -= 3;
          }
       else if (totalHours >= 2)
   {
    sessions.Add(2);
          totalHours -= 2;
            }
      else
            {
         sessions.Add(1);
       totalHours -= 1;
            }
        }

        return sessions;
    }

    private bool HasConsecutiveSlots(
        DayOfWeek day,
        TimeSpan startTime,
    int hoursNeeded,
   Dictionary<string, bool> usedSlots)
{
      for (int i = 0; i < hoursNeeded; i++)
        {
         var slotTime = startTime.Add(TimeSpan.FromHours(i));
    var slotKey = $"{day}_{slotTime}";

            if (usedSlots.ContainsKey(slotKey))
       return false;

      // Check if slot exists in available time slots
if (!_timeSlots.Contains(slotTime))
       return false;
}

        return true;
    }

    public async Task<bool> HasConflictAsync(CourseSchedule schedule)
    {
        var conflicts = await _db.CourseSchedules
      .Where(cs =>
              cs.Id != schedule.Id &&
            cs.Semester == schedule.Semester &&
       cs.DayOfWeek == schedule.DayOfWeek &&
   ((cs.StartTime < schedule.EndTime && cs.EndTime > schedule.StartTime) ||
       (schedule.StartTime < cs.EndTime && schedule.EndTime > cs.StartTime)))
   .AnyAsync();

   return conflicts;
    }

  public async Task<List<ScheduleConflict>> DetectConflictsAsync(int semester)
    {
        var schedules = await _db.CourseSchedules
   .Include(cs => cs.Course)
    .Where(cs => cs.Semester == semester)
.ToListAsync();

        var conflicts = new List<ScheduleConflict>();

      for (int i = 0; i < schedules.Count; i++)
        {
            for (int j = i + 1; j < schedules.Count; j++)
     {
  var s1 = schedules[i];
  var s2 = schedules[j];

       if (s1.DayOfWeek == s2.DayOfWeek &&
    s1.StartTime < s2.EndTime &&
         s2.StartTime < s1.EndTime)
   {
               conflicts.Add(new ScheduleConflict
         {
       Schedule1Id = s1.Id,
              Schedule2Id = s2.Id,
    ConflictType = "TimeOverlap",
            Description = $"{s1.Course.CourseCode} and {s2.Course.CourseCode} overlap on {s1.DayOfWeek}",
   DetectedAt = DateTime.UtcNow
            });
        }
   }
     }

        return conflicts;
    }
}
