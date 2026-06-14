using AdvisorySystem.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdvisorySystem.Api.Services;

public static class CourseSeeder
{
    public static async Task SeedCoursesAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var existingCourses = await db.Courses.ToListAsync();
        if (existingCourses.Any())
        {
     var prerequisites = await db.Prerequisites.ToListAsync();
    db.Prerequisites.RemoveRange(prerequisites);
var schedules = await db.CourseSchedules.ToListAsync();
     db.CourseSchedules.RemoveRange(schedules);
            var studentSections = await db.StudentCourseSections.ToListAsync();
            db.StudentCourseSections.RemoveRange(studentSections);
   var studentCourses = await db.StudentCourses.ToListAsync();
            db.StudentCourses.RemoveRange(studentCourses);
        db.Courses.RemoveRange(existingCourses);
        var existingCategories = await db.CourseCategories.ToListAsync();
 db.CourseCategories.RemoveRange(existingCategories);
        await db.SaveChangesAsync();
  }

        var categories = new List<CourseCategory>
        {
            new() { Name = "Universite Zorunlu Dersleri",  Description = "Tum ogrenciler icin zorunlu",     DisplayOrder = 1  },
            new() { Name = "Birinci Yariyil (Guz)",         Description = "1. Sinif Guz Donemi",      DisplayOrder = 2  },
        new() { Name = "Ikinci Yariyil (Bahar)",        Description = "1. Sinif Bahar Donemi",     DisplayOrder = 3  },
  new() { Name = "Ucuncu Yariyil (Guz)",  Description = "2. Sinif Guz Donemi",             DisplayOrder = 4  },
        new() { Name = "Dorduncu Yariyil (Bahar)",      Description = "2. Sinif Bahar Donemi",         DisplayOrder = 5  },
            new() { Name = "Besinci Yariyil (Guz)",         Description = "3. Sinif Guz Donemi",     DisplayOrder = 6  },
        new() { Name = "Altinci Yariyil (Bahar)",    Description = "3. Sinif Bahar Donemi",       DisplayOrder = 7  },
     new() { Name = "Yedinci Yariyil (Guz)",         Description = "4. Sinif Guz Donemi",     DisplayOrder = 8  },
   new() { Name = "Sekizinci Yariyil (Bahar)",     Description = "4. Sinif Bahar Donemi",           DisplayOrder = 9  },
     new() { Name = "Teknik Secmeli Dersler",        Description = "Teknik alanda secmeli dersler",             DisplayOrder = 10 },
            new() { Name = "Sosyal Secmeli Dersler",        Description = "Sosyal bilimler secmeli dersler",       DisplayOrder = 11 },
 new() { Name = "Ortak Secmeli Dersler",         Description = "Guzel sanatlar ve genel secmeli dersler",   DisplayOrder = 12 },
        new() { Name = "Katalog Disi Secmeli Ders",     Description = "Ozel secmeli dersler",    DisplayOrder = 13 },
        };

        await db.CourseCategories.AddRangeAsync(categories);
      await db.SaveChangesAsync();

        var savedCats = await db.CourseCategories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var cid = savedCats
  .GroupBy(c => c.DisplayOrder)
    .ToDictionary(g => g.Key, g => g.First().Id);

        var courses = new List<Course>
        {
 // Universite Zorunlu Dersleri (DisplayOrder=1)
  new() { CourseCode = "KRY100",   CourseName = "KARIYER PLANLAMA",               TheoryHours = 1, PracticeHours = 0, Credits = 1, ECTS = 2, CategoryId = cid[1]  },
            new() { CourseCode = "ORY100",   CourseName = "UNIVERSITE HAYATINA GIRIS",     TheoryHours = 1, PracticeHours = 0, Credits = 1, ECTS = 1, CategoryId = cid[1]  },

  // 1. Yariyil (DisplayOrder=2)
            new() { CourseCode = "BIL101", CourseName = "BILGISAYAR YAZILIMI I",        TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[2],  Semester = 1 },
          new() { CourseCode = "BIL105",   CourseName = "PROGRAMLAMA LABORATUVARI I",              TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = cid[2],  Semester = 1 },
    new() { CourseCode = "BIL110",   CourseName = "BILGISAYAR MUHENDISLIGINE GIRIS",TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 4, CategoryId = cid[2],  Semester = 1 },
    new() { CourseCode = "ENG199",   CourseName = "ADVANCED ENGLISH I",   TheoryHours = 4, PracticeHours = 0, Credits = 4, ECTS = 4, CategoryId = cid[2],  Semester = 1 },
  new() { CourseCode = "FIZ103",   CourseName = "MEKANIK LABORATUVARI",         TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = cid[2],  Semester = 1 },
          new() { CourseCode = "FIZ105",   CourseName = "GENEL FIZIK I",     TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[2],  Semester = 1 },
          new() { CourseCode = "MAT151",   CourseName = "MATEMATIKSEL ANALIZ I",     TheoryHours = 4, PracticeHours = 1, Credits = 4, ECTS = 6, CategoryId = cid[2],  Semester = 1 },
     new() { CourseCode = "TURK101",  CourseName = "TURK DILI I",       TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = cid[2],  Semester = 1 },

      // 2. Yariyil (DisplayOrder=3)
            new() { CourseCode = "BIL122",CourseName = "ILERI PROGRAMLAMA",      TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[3],  Semester = 2 },
     new() { CourseCode = "BIL124",   CourseName = "ILERI PROGRAMLAMA UYGULAMALARI",      TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = cid[3],  Semester = 2 },
            new() { CourseCode = "BIL172",   CourseName = "YASAM BILIMLERI VE BILGISAYAR MUHENDISLIGI",    TheoryHours = 2, PracticeHours = 1, Credits = 2, ECTS = 4, CategoryId = cid[3],  Semester = 2 },
  new() { CourseCode = "FIZ104",   CourseName = "ELEKTRIK LABORATUVARI",      TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = cid[3],  Semester = 2 },
            new() { CourseCode = "FIZ110",   CourseName = "GENEL FIZIK II",      TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[3],  Semester = 2 },
            new() { CourseCode = "MAT152",   CourseName = "MATEMATIKSEL ANALIZ II",          TheoryHours = 4, PracticeHours = 1, Credits = 4, ECTS = 6, CategoryId = cid[3],  Semester = 2 },
    new() { CourseCode = "MAT210",   CourseName = "DOGRUSAL CEBIR",          TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = cid[3],  Semester = 2 },
 new() { CourseCode = "TURK102",  CourseName = "TURK DILI II",        TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = cid[3],  Semester = 2 },

// 3. Yariyil (DisplayOrder=4)
            new() { CourseCode = "ATA201",   CourseName = "ATATURK ILKELERI VE INKILAP TARIHI I",            TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = cid[4],  Semester = 3 },
            new() { CourseCode = "BIL231",   CourseName = "AYRIK YAPILAR",     TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = cid[4],  Semester = 3 },
     new() { CourseCode = "BIL265",   CourseName = "VERI YAPILARI",           TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 7, CategoryId = cid[4],  Semester = 3 },
    new() { CourseCode = "BIL275", CourseName = "SAYISAL MANTIK TASARIMI",            TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = cid[4],  Semester = 3 },
 new() { CourseCode = "ENG200",   CourseName = "ADVANCED ENGLISH II",         TheoryHours = 4, PracticeHours = 0, Credits = 4, ECTS = 4, CategoryId = cid[4],  Semester = 3 },
          new() { CourseCode = "SOS203",   CourseName = "EKONOMI",   TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 4, CategoryId = cid[4],  Semester = 3 },

   // 4. Yariyil (DisplayOrder=5)
       new() { CourseCode = "ATA202",   CourseName = "ATATURK ILKELERI VE INKILAP TARIHI II",        TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = cid[5],  Semester = 4 },
        new() { CourseCode = "BIL210",   CourseName = "ELEKTRONIGE GIRIS",   TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 6, CategoryId = cid[5],  Semester = 4 },
         new() { CourseCode = "BIL218",   CourseName = "BILGISAYAR ORGANIZASYONU",                TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = cid[5],  Semester = 4 },
            new() { CourseCode = "BIL240",   CourseName = "PROGRAMLAMA DILLERI",      TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = cid[5],  Semester = 4 },
      new() { CourseCode = "MAT250",   CourseName = "OLASILIK VE ISTATISTIK",      TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[5],  Semester = 4 },
new() { CourseCode = "MAT286",   CourseName = "BILGISAYAR MUHENDISLIGI ICIN MATEMATIK",             TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[5],  Semester = 4 },

            // 5. Yariyil (DisplayOrder=6)
new() { CourseCode = "BIL300",   CourseName = "STAJ I",       TheoryHours = 0, PracticeHours = 0, Credits = 0, ECTS = 2, CategoryId = cid[6],  Semester = 5 },
            new() { CourseCode = "BIL324",   CourseName = "MIKROISLEMCILER",      TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 5, CategoryId = cid[6],  Semester = 5 },
            new() { CourseCode = "BIL343",   CourseName = "NESNE YONELIMLI PROGRAMLAMA",     TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[6],  Semester = 5 },
            new() { CourseCode = "BIL367",   CourseName = "ALGORITMALAR",  TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[6],  Semester = 5 },
       new() { CourseCode = "ENG330",   CourseName = "DEVELOPING ENGLISH LANGUAGE SKILLS",      TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = cid[6],  Semester = 5 },
 new() { CourseCode = "MAT311",   CourseName = "SAYISAL ANALIZ TEKNIKLERI",    TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 5, CategoryId = cid[6],  Semester = 5 },
 new() { CourseCode = "SOS204",   CourseName = "ISLETME",    TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 4, CategoryId = cid[6],  Semester = 5 },

            // 6. Yariyil (DisplayOrder=7)
      new() { CourseCode = "BIL001",   CourseName = "TEKNIK SECMELIK I",       TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[7],  Semester = 6, IsElective = true },
            new() { CourseCode = "BIL007",   CourseName = "SOSYAL SECMELIK",    TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = cid[7],  Semester = 6, IsElective = true },
      new() { CourseCode = "BIL332",   CourseName = "ISLETIM SISTEMLERI",     TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 7, CategoryId = cid[7],  Semester = 6 },
            new() { CourseCode = "BIL344",   CourseName = "VERITABANI SISTEMLERI",     TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = cid[7],  Semester = 6 },
   new() { CourseCode = "BIL386",   CourseName = "YAZILIM MUHENDISLIGINE GIRIS",          TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = cid[7],  Semester = 6 },
 new() { CourseCode = "GSBHSH",   CourseName = "SECMELI GUZEL SANATLAR/ILK YARDIM",                  TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[7],  Semester = 6, IsElective = true },

            // 7. Yariyil (DisplayOrder=8)
            new() { CourseCode = "BIL002",   CourseName = "TEKNIK SECMELIK II",        TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[8],  Semester = 7, IsElective = true },
            new() { CourseCode = "BIL003",CourseName = "TEKNIK SECMELIK III",    TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[8],  Semester = 7, IsElective = true },
      new() { CourseCode = "BIL493",   CourseName = "BITIRME PROJESI I",   TheoryHours = 0, PracticeHours = 4, Credits = 2, ECTS = 7, CategoryId = cid[8],  Semester = 7 },
   new() { CourseCode = "BIL498",   CourseName = "STAJ II", TheoryHours = 0, PracticeHours = 0, Credits = 0, ECTS = 3, CategoryId = cid[8],  Semester = 7 },
        new() { CourseCode = "BIL499",   CourseName = "BILGISAYAR AGLARI",TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 6, CategoryId = cid[8],  Semester = 7 },
new() { CourseCode = "ENG460",   CourseName = "PRESENTATION SKILLS",       TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = cid[8],  Semester = 7 },

      // 8. Yariyil (DisplayOrder=9)
            new() { CourseCode = "BIL004",   CourseName = "TEKNIK SECMELIK IV",        TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[9],  Semester = 8, IsElective = true },
 new() { CourseCode = "BIL005",   CourseName = "TEKNIK SECMELIK V",  TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[9],  Semester = 8, IsElective = true },
    new() { CourseCode = "BIL006",   CourseName = "TEKNIK SECMELIK VI",  TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[9],  Semester = 8, IsElective = true },
          new() { CourseCode = "BIL482",   CourseName = "ETIK, TOPLUM VE MESLEK",            TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 7, CategoryId = cid[9],  Semester = 8 },
            new() { CourseCode = "BIL494",   CourseName = "BITIRME PROJESI II",        TheoryHours = 0, PracticeHours = 4, Credits = 2, ECTS = 8, CategoryId = cid[9],  Semester = 8 },

// Teknik Secmeli (DisplayOrder=10)
            new() { CourseCode = "BIL321",   CourseName = "HESAPLAMALI GRAFIK",   TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL328",   CourseName = "OTOMATA TEORISI", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
         new() { CourseCode = "BIL345",   CourseName = "SISTEM MUHENDISLIGI",             TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
 new() { CourseCode = "BIL363",   CourseName = "INSAN BILGISAYAR ETKILESIMI",    TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
   new() { CourseCode = "BIL383",   CourseName = "YONETIM BILISIM SISTEMLERI",        TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
      new() { CourseCode = "BIL387",   CourseName = "WEB TASARIMI VE UYGULAMALARI",             TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
      new() { CourseCode = "BIL388",   CourseName = "E-TICARET VE E-IS",    TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
   new() { CourseCode = "BIL389",   CourseName = "BILGISAYAR AG UYGULAMALARI",     TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL390",   CourseName = "YAZILIM KALITE YONETIMI",                TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
   new() { CourseCode = "BIL391",   CourseName = "MOBIL UYGULAMA GELISTIRME",   TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL392",   CourseName = "OYUN TEKNOLOJILERI",TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
 new() { CourseCode = "BIL393",   CourseName = "YONEYLEM ARASTIRMASI",     TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL395",   CourseName = "UYGULAMALI VERI ANALIZI",      TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
         new() { CourseCode = "BIL396",   CourseName = "BILGISAYAR BILIMLERINDE GUNCEL KONULAR",             TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL443",   CourseName = "KRIPTOGRAFI VE GUVENLIK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
     new() { CourseCode = "BIL447",   CourseName = "GOMULU SISTEMLER",          TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
    new() { CourseCode = "BIL454",   CourseName = "UYGULAMALI UML",          TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL456",   CourseName = "IMGE ISLEME",        TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
new() { CourseCode = "BIL457",   CourseName = "KUANTUM HESAPLAMAYA GIRIS",       TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
    new() { CourseCode = "BIL458",   CourseName = "BULUT COZUME GIRIS",         TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
          new() { CourseCode = "BIL459",   CourseName = "YAZILIM MIMARILERI",    TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
          new() { CourseCode = "BIL466",   CourseName = "BIYOBILISIM",             TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
  new() { CourseCode = "BIL471",   CourseName = "BILGISAYARLA GORME",        TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
     new() { CourseCode = "BIL473",   CourseName = "TIP BILISIMI",          TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
       new() { CourseCode = "BIL475",   CourseName = "CIZGE KURAMI",   TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
         new() { CourseCode = "BIL477",   CourseName = "VERI MADENCILIGINE GIRIS",          TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
  new() { CourseCode = "BIL478",   CourseName = "PARALEL VERI ISLEME",         TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
        new() { CourseCode = "BIL479",   CourseName = "ORUNTU TANIMA",        TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL480",   CourseName = "YAPAY ZEKA",   TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL481",   CourseName = "BILGISAYAR MUHENDISLIGINDE OZEL KONULAR",            TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL489",   CourseName = "COKLUORTAM SISTEMLERI",             TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL490",   CourseName = "SAYISAL YONTEMLER VE OPTIMIZASYON",      TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL495",   CourseName = "UNIX SISTEM PROGRAMLAMA",         TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },
            new() { CourseCode = "BIL497",   CourseName = "GERCEK ZAMANLI SISTEMLER",           TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = cid[10], IsElective = true },

      // Sosyal Secmeli (DisplayOrder=11)
    new() { CourseCode = "SOS321",   CourseName = "ILETISIM",     TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = cid[11], IsElective = true },
 new() { CourseCode = "SOS322",   CourseName = "ISLETME YONETIMINE GIRIS",            TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = cid[11], IsElective = true },

            // Ortak Secmeli (DisplayOrder=12)
    new() { CourseCode = "GSB101",   CourseName = "FOTOGRAFCILIK",           TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
    new() { CourseCode = "GSB102",   CourseName = "FOTOGRAFCILIK II", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
 new() { CourseCode = "GSB103",   CourseName = "HEYKEL",    TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB104",   CourseName = "HEYKEL II",           TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB105",   CourseName = "KLASIK MUZIK DINLEME KULTURU",              TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB107",   CourseName = "GORSEL KULTUR VE SANATIN TARIHI",             TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB109",CourseName = "ANADOLU ARKEOLOJISI",     TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
          new() { CourseCode = "GSB111",   CourseName = "SINEMA KULTURU VE TARIHI",     TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
new() { CourseCode = "GSB113",   CourseName = "RESIM",          TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
    new() { CourseCode = "GSB115",   CourseName = "DOGACLAMA VE ILISKILI DOGACLAMA",     TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB117",   CourseName = "CAGDAS DANSI ANLAMA VE YORUMLAMA",            TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB119",   CourseName = "INSAN VE CEVRE ETKILESIMI",        TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB121",   CourseName = "TASARIM KULTUR VE TUKETIM",   TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
     new() { CourseCode = "GSB123",   CourseName = "CAGDAS DANSA GIRIS",          TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
 new() { CourseCode = "GSB125",   CourseName = "TAKI TASARIMI",   TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB127",   CourseName = "SERAMIK",      TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
new() { CourseCode = "GSB129",   CourseName = "KENTLER VE TARIHSEL CEVRE",   TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB131",   CourseName = "21. YUZYILDA DUNYA VE SANATTA EGILIMLER",     TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "GSB133",   CourseName = "CAGLAR BOYU MUZIK TURLERI",  TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
     new() { CourseCode = "GSB135",   CourseName = "SANAT VE EDEBIYAT ESERLERINDE EVRENSEL HUKUK ILKELERI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
        new() { CourseCode = "GSB137",   CourseName = "ETKILI VE GUZEL KONUSMA",    TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
      new() { CourseCode = "GSB139",   CourseName = "GUNCEL TEMEL EKONOMI",    TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
      new() { CourseCode = "HSH100",   CourseName = "TEMEL ILK YARDIM",       TheoryHours = 1, PracticeHours = 1, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "KAM100",   CourseName = "KIBRIS TURKLERININ YAKIN TARIHI",         TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
   new() { CourseCode = "TIP099",   CourseName = "TOPLUMSAL CINSIYET VE KADINA YONELIK SIDDET",     TheoryHours = 1, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },
            new() { CourseCode = "YAKE100",  CourseName = "YARATICI KULTUR ENDUSTRILERI",           TheoryHours = 2, PracticeHours = 0, Credits = 1, ECTS = 1, CategoryId = cid[12], IsElective = true },

   // Katalog Disi (DisplayOrder=13)
 new() { CourseCode = "GNLI310",  CourseName = "GONULLULUK CALISMALARI",      TheoryHours = 1, PracticeHours = 2, Credits = 2, ECTS = 4, CategoryId = cid[13], IsElective = true },
        };

   await db.Courses.AddRangeAsync(courses);
      await db.SaveChangesAsync();

await AddPrerequisites(db);
    }

    private static async Task AddPrerequisites(AppDbContext db)
    {
   var bil122 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL122");
 var bil101 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL101");

        var mat152 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT152");
        var mat151 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT151");

        var bil265 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL265");

        var eng200 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "ENG200");
        var eng199 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "ENG199");

        var bil210 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL210");
        var fiz110 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "FIZ110");

        var bil218 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL218");
        var bil275 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL275");

      var bil240 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL240");

        var mat286 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT286");

        var bil367 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL367");

  var mat311 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT311");

        var bil390 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL390");
        var bil386 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL386");

        var bil498 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL498");
     var bil300 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL300");

        var bil494 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL494");
        var bil493 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BIL493");

        var prerequisites = new List<Prerequisite>();

        if (bil122 != null && bil101 != null)
            prerequisites.Add(new Prerequisite { CourseId = bil122.Id, PrerequisiteCourseId = bil101.Id });
        if (mat152 != null && mat151 != null)
            prerequisites.Add(new Prerequisite { CourseId = mat152.Id, PrerequisiteCourseId = mat151.Id });
        if (bil265 != null && bil122 != null)
            prerequisites.Add(new Prerequisite { CourseId = bil265.Id, PrerequisiteCourseId = bil122.Id });
        if (eng200 != null && eng199 != null)
            prerequisites.Add(new Prerequisite { CourseId = eng200.Id, PrerequisiteCourseId = eng199.Id });
        if (bil210 != null && fiz110 != null)
            prerequisites.Add(new Prerequisite { CourseId = bil210.Id, PrerequisiteCourseId = fiz110.Id });
if (bil218 != null && bil275 != null)
prerequisites.Add(new Prerequisite { CourseId = bil218.Id, PrerequisiteCourseId = bil275.Id });
        if (bil240 != null && bil122 != null)
       prerequisites.Add(new Prerequisite { CourseId = bil240.Id, PrerequisiteCourseId = bil122.Id });
        if (mat286 != null && mat152 != null)
            prerequisites.Add(new Prerequisite { CourseId = mat286.Id, PrerequisiteCourseId = mat152.Id });
        if (bil367 != null && bil265 != null)
            prerequisites.Add(new Prerequisite { CourseId = bil367.Id, PrerequisiteCourseId = bil265.Id });
        if (mat311 != null && mat151 != null)
       prerequisites.Add(new Prerequisite { CourseId = mat311.Id, PrerequisiteCourseId = mat151.Id });
        if (bil390 != null && bil386 != null)
       prerequisites.Add(new Prerequisite { CourseId = bil390.Id, PrerequisiteCourseId = bil386.Id });
        if (bil498 != null && bil300 != null)
 prerequisites.Add(new Prerequisite { CourseId = bil498.Id, PrerequisiteCourseId = bil300.Id });
        if (bil494 != null && bil493 != null)
   prerequisites.Add(new Prerequisite { CourseId = bil494.Id, PrerequisiteCourseId = bil493.Id });

        if (prerequisites.Any())
        {
        await db.Prerequisites.AddRangeAsync(prerequisites);
    await db.SaveChangesAsync();
     }
    }
}
