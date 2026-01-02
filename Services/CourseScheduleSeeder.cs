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

        // Zaman dilimleri (08:00-17:00 arasý, her ders 110 dakika)
        var timeSlots = new[]
        {
          (new TimeSpan(8, 30, 0), new TimeSpan(10, 20, 0)),   // 08:30-10:20
            (new TimeSpan(10, 30, 0), new TimeSpan(12, 20, 0)),  // 10:30-12:20
     (new TimeSpan(13, 00, 0), new TimeSpan(14, 50, 0)),  // 13:00-14:50
         (new TimeSpan(15, 00, 0), new TimeSpan(16, 50, 0))   // 15:00-16:50
     };

   var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var instructors = new[] 
  { 
     "Prof. Dr. Ahmet Yýlmaz", 
  "Doç. Dr. Ayþe Demir", 
            "Dr. Öðr. Üyesi Mehmet Kaya",
      "Arþ. Gör. Fatma Çelik",
    "Öðr. Gör. Can Þahin"
        };

        int currentTimeSlotIndex = 0;
     int currentDayIndex = 0;

        foreach (var course in courses.OrderBy(c => c.Semester).ThenBy(c => c.CourseCode))
        {
     var semester = course.Semester ?? 1;
      var totalSessions = course.TheoryHours + course.PracticeHours;
         
            // Teorik dersler için
       if (course.TheoryHours > 0)
            {
             var sessionsNeeded = (int)Math.Ceiling(course.TheoryHours / 2.0);
                
     for (int i = 0; i < sessionsNeeded; i++)
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

       // Uygulama dersleri için (lab)
     if (course.PracticeHours > 0)
       {
        var sessionsNeeded = (int)Math.Ceiling(course.PracticeHours / 2.0);
    
                for (int i = 0; i < sessionsNeeded; i++)
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
        // Mevcut gün ve saat diliminden baþla
        for (int d = 0; d < days.Length; d++)
        {
            var day = days[(dayIndex + d) % days.Length];
        
   for (int t = 0; t < timeSlots.Length; t++)
            {
   var timeSlot = timeSlots[(timeIndex + t) % timeSlots.Length];
 var key = (semester, day, timeSlot.start);
        
       if (!usedSlots.ContainsKey(key))
 {
        // Sonraki slot için index'i güncelle
timeIndex = (timeIndex + t + 1) % timeSlots.Length;
   if (timeIndex == 0)
      {
 dayIndex = (dayIndex + d + 1) % days.Length;
         }
    
              return (day, timeSlot.start, timeSlot.end);
      }
         }
        }

        // Eðer hiç boþ slot yoksa (çok nadir), varsayýlan deðer döndür
        dayIndex = (dayIndex + 1) % days.Length;
        timeIndex = (timeIndex + 1) % timeSlots.Length;
    return (days[dayIndex], timeSlots[timeIndex].start, timeSlots[timeIndex].end);
    }
}
