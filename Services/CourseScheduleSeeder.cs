using AdvisorySystem.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdvisorySystem.Api.Services;

public static class CourseScheduleSeeder
{
    public static async Task SeedSchedulesAsync(IServiceProvider sp)
 {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

     if (await db.CourseSchedules.AnyAsync())
    {
          Console.WriteLine("??  Schedules already exist. Skipping seeding.");
 return;
      }

    Console.WriteLine("?? Creating course schedules...");

        var courses = await db.Courses
      .Where(c => c.Semester != null && !c.IsElective)
    .ToListAsync();

        var schedules = new List<CourseSchedule>();
  var usedTimeSlots = new Dictionary<(int semester, DayOfWeek day, TimeSpan time), bool>();

        // ? Tam saatler - Her ders 60 dakika (08:00-17:00 arasý)
        var timeSlots = new[]
        {
            (new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0)),    // 08:00-09:00
    (new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)),   // 09:00-10:00
            (new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0)),  // 10:00-11:00
  (new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0)),  // 11:00-12:00
            (new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0)),  // 13:00-14:00
            (new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0)),  // 14:00-15:00
     (new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0)),  // 15:00-16:00
         (new TimeSpan(16, 0, 0), new TimeSpan(17, 0, 0))   // 16:00-17:00
        };

        var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var instructors = new[] 
        { 
  "Prof. Dr. Ahmet Yilmaz", 
      "Doc. Dr. Ayse Demir", 
       "Dr. Ogr. Uyesi Mehmet Kaya",
       "Ars. Gor. Fatma Celik",
       "Ogr. Gor. Can Sahin"
   };

        int currentTimeSlotIndex = 0;
        int currentDayIndex = 0;

      foreach (var course in courses.OrderBy(c => c.Semester).ThenBy(c => c.CourseCode))
        {
      var semester = course.Semester ?? 1;
     
            // Teorik dersler için - Her saat 1 ders saati
       if (course.TheoryHours > 0)
    {
   for (int i = 0; i < course.TheoryHours; i++)
    {
       var (day, startTime, endTime) = FindAvailableSlot(
        semester, 
 days, 
 timeSlots, 
        usedTimeSlots, 
    ref currentDayIndex, 
  ref currentTimeSlotIndex
          );

                    var schedule = new CourseSchedule
  {
     CourseId = course.Id,
        Semester = semester,
            SectionCode = "A",
               DayOfWeek = day,
             StartTime = startTime,
           EndTime = endTime,
       RoomNumber = $"A{100 + (schedules.Count % 20)}",
         InstructorName = instructors[schedules.Count % instructors.Length],
      IsTheory = true,
       SessionNumber = i + 1,
        MaxCapacity = 50
 };

         schedules.Add(schedule);
       usedTimeSlots[(semester, day, startTime)] = true;
     }
     }

      // Uygulama dersleri için (lab) - Her saat 1 ders saati
          if (course.PracticeHours > 0)
    {
        for (int i = 0; i < course.PracticeHours; i++)
        {
      var (day, startTime, endTime) = FindAvailableSlot(
       semester, 
  days, 
               timeSlots, 
    usedTimeSlots, 
       ref currentDayIndex, 
         ref currentTimeSlotIndex
         );

  var schedule = new CourseSchedule
         {
   CourseId = course.Id,
              Semester = semester,
    SectionCode = "A",
        DayOfWeek = day,
  StartTime = startTime,
     EndTime = endTime,
      RoomNumber = $"LAB{1 + (schedules.Count % 5)}",
                 InstructorName = instructors[schedules.Count % instructors.Length],
       IsTheory = false,
    SessionNumber = i + 1,
    MaxCapacity = 30
                    };

      schedules.Add(schedule);
              usedTimeSlots[(semester, day, startTime)] = true;
     }
          }
        }

        await db.CourseSchedules.AddRangeAsync(schedules);
        await db.SaveChangesAsync();

        Console.WriteLine($"? Created {schedules.Count} course schedules!");
  }

    private static (DayOfWeek day, TimeSpan start, TimeSpan end) FindAvailableSlot(
        int semester,
        DayOfWeek[] days,
    (TimeSpan start, TimeSpan end)[] timeSlots,
      Dictionary<(int semester, DayOfWeek day, TimeSpan time), bool> usedSlots,
        ref int dayIndex,
   ref int timeIndex)
    {
        for (int d = 0; d < days.Length; d++)
        {
  var day = days[(dayIndex + d) % days.Length];
  
     for (int t = 0; t < timeSlots.Length; t++)
            {
      var timeSlot = timeSlots[(timeIndex + t) % timeSlots.Length];
          var key = (semester, day, timeSlot.start);
        
if (!usedSlots.ContainsKey(key))
   {
   timeIndex = (timeIndex + t + 1) % timeSlots.Length;
 if (timeIndex == 0)
        {
            dayIndex = (dayIndex + d + 1) % days.Length;
         }
        
                 return (day, timeSlot.start, timeSlot.end);
                }
            }
      }

        dayIndex = (dayIndex + 1) % days.Length;
timeIndex = (timeIndex + 1) % timeSlots.Length;
  return (days[dayIndex], timeSlots[timeIndex].start, timeSlots[timeIndex].end);
    }
}
