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

        if (await db.Courses.AnyAsync())
      {
          return;
 }

        var categories = new List<CourseCategory>
   {
         new() { Name = "Üniversite Zorunlu Dersleri", Description = "Tüm öðrenciler için zorunlu", DisplayOrder = 1 },
            new() { Name = "Birinci Yarýyýl (Güz)", Description = "1. Sýnýf Güz Dönemi", DisplayOrder = 2 },
 new() { Name = "Ýkinci Yarýyýl (Bahar)", Description = "1. Sýnýf Bahar Dönemi", DisplayOrder = 3 },
       new() { Name = "Üçüncü Yarýyýl (Güz)", Description = "2. Sýnýf Güz Dönemi", DisplayOrder = 4 },
        new() { Name = "Dördüncü Yarýyýl (Bahar)", Description = "2. Sýnýf Bahar Dönemi", DisplayOrder = 5 },
         new() { Name = "Beþinci Yarýyýl (Güz)", Description = "3. Sýnýf Güz Dönemi", DisplayOrder = 6 },
          new() { Name = "Altýncý Yarýyýl (Bahar)", Description = "3. Sýnýf Bahar Dönemi", DisplayOrder = 7 },
          new() { Name = "Yedinci Yarýyýl (Güz)", Description = "4. Sýnýf Güz Dönemi", DisplayOrder = 8 },
new() { Name = "Sekizinci Yarýyýl (Bahar)", Description = "4. Sýnýf Bahar Dönemi", DisplayOrder = 9 },
 new() { Name = "Teknik Seçmeli Dersler", Description = "Teknik alanda seçmeli dersler", DisplayOrder = 10 },
            new() { Name = "Sosyal Seçmeli Dersler", Description = "Sosyal bilimler seçmeli dersler", DisplayOrder = 11 },
            new() { Name = "Ortak Seçmeli Dersler", Description = "Güzel sanatlar ve genel seçmeli dersler", DisplayOrder = 12 },
            new() { Name = "Katalog Dýþý Seçmeli Ders", Description = "Özel seçmeli dersler", DisplayOrder = 13 }
};

      await db.CourseCategories.AddRangeAsync(categories);
      await db.SaveChangesAsync();

        var courses = new List<Course>
  {
       // Üniversite Zorunlu Dersleri
      new() { CourseCode = "KRY100", CourseName = "KARÝYER PLANLAMA", TheoryHours = 1, PracticeHours = 0, Credits = 1, ECTS = 2, CategoryId = 1 },
    new() { CourseCode = "ORY100", CourseName = "ÜNÝVERSÝTE HAYATINA GÝRÝÞ", TheoryHours = 1, PracticeHours = 0, Credits = 1, ECTS = 1, CategoryId = 1 },

       // Birinci Yarýyýl (Güz) - Semester 1
            new() { CourseCode = "BÝL101", CourseName = "BÝLGÝSAYAR YAZILIMI I", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 2, Semester = 1 },
            new() { CourseCode = "BÝL105", CourseName = "PROGRAMLAMA LABORATUVARI I", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 2, Semester = 1 },
         new() { CourseCode = "BÝL110", CourseName = "BÝLGÝSAYAR MÜHENDÝSLÝÐÝNE GÝRÝÞ", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 4, CategoryId = 2, Semester = 1 },
  new() { CourseCode = "ENG199", CourseName = "ADVANCED ENGLISH I", TheoryHours = 4, PracticeHours = 0, Credits = 4, ECTS = 4, CategoryId = 2, Semester = 1 },
            new() { CourseCode = "FÝZ103", CourseName = "MEKANÝK LABORATUVARI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 2, Semester = 1 },
 new() { CourseCode = "FÝZ105", CourseName = "GENEL FÝZÝK I", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 2, Semester = 1 },
      new() { CourseCode = "MAT151", CourseName = "MATEMATÝKSEL ANALÝZ I", TheoryHours = 4, PracticeHours = 1, Credits = 4, ECTS = 6, CategoryId = 2, Semester = 1 },
            new() { CourseCode = "TÜRK101", CourseName = "TÜRK DÝLÝ I", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 2, Semester = 1 },

  // Ýkinci Yarýyýl (Bahar) - Semester 2
  new() { CourseCode = "BÝL122", CourseName = "ÝLERÝ PROGRAMLAMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 3, Semester = 2 },
    new() { CourseCode = "BÝL124", CourseName = "ÝLERÝ PROGRAMLAMA UYGULAMALARI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 3, Semester = 2 },
            new() { CourseCode = "BÝL172", CourseName = "YAÞAM BÝLÝMLERÝ VE BÝLGÝSAYAR MÜHENDÝSLÝÐÝ", TheoryHours = 2, PracticeHours = 1, Credits = 2, ECTS = 4, CategoryId = 3, Semester = 2 },
        new() { CourseCode = "FÝZ104", CourseName = "ELEKTRÝK LABORATUVARI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 3, Semester = 2 },
    new() { CourseCode = "FÝZ110", CourseName = "GENEL FÝZÝK II", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 3, Semester = 2 },
        new() { CourseCode = "MAT152", CourseName = "MATEMATÝKSEL ANALÝZ II", TheoryHours = 4, PracticeHours = 1, Credits = 4, ECTS = 6, CategoryId = 3, Semester = 2 },
            new() { CourseCode = "MAT210", CourseName = "DOÐRUSAL CEBÝR", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = 3, Semester = 2 },
       new() { CourseCode = "TÜRK102", CourseName = "TÜRK DÝLÝ II", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 3, Semester = 2 },

       // Üçüncü Yarýyýl (Güz) - Semester 3
 new() { CourseCode = "ATA201", CourseName = "ATATÜRK ÝLKELERÝ VE ÝNKILAP TARÝHÝ I", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 4, Semester = 3 },
      new() { CourseCode = "BÝL231", CourseName = "AYRIK YAPILAR", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = 4, Semester = 3 },
            new() { CourseCode = "BÝL265", CourseName = "VERÝ YAPILARI", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 7, CategoryId = 4, Semester = 3 },
    new() { CourseCode = "BÝL275", CourseName = "SAYISAL MANTIK TASARIMI", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = 4, Semester = 3 },
      new() { CourseCode = "ENG200", CourseName = "ADVANCED ENGLISH II", TheoryHours = 4, PracticeHours = 0, Credits = 4, ECTS = 4, CategoryId = 4, Semester = 3 },
            new() { CourseCode = "SOS203", CourseName = "EKONOMÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 4, CategoryId = 4, Semester = 3 },

            // Dördüncü Yarýyýl (Bahar) - Semester 4
            new() { CourseCode = "ATA202", CourseName = "ATATÜRK ÝLKELERÝ VE ÝNKILAP TARÝHÝ II", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 5, Semester = 4 },
            new() { CourseCode = "BÝL210", CourseName = "ELEKTRONÝÐE GÝRÝÞ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 6, CategoryId = 5, Semester = 4 },
 new() { CourseCode = "BÝL218", CourseName = "BÝLGÝSAYAR ORGANÝZASYONU", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = 5, Semester = 4 },
   new() { CourseCode = "BÝL240", CourseName = "PROGRAMLAMA DÝLLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = 5, Semester = 4 },
  new() { CourseCode = "MAT250", CourseName = "OLASILIK VE ÝSTATÝSTÝK", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 5, Semester = 4 },
      new() { CourseCode = "MAT286", CourseName = "BÝLGÝSAYAR MÜHENDÝSLÝÐÝ ÝÇÝN MATEMATÝK", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 5, Semester = 4 },

   // Beþinci Yarýyýl (Güz) - Semester 5
    new() { CourseCode = "BÝL300", CourseName = "STAJ I", TheoryHours = 0, PracticeHours = 0, Credits = 0, ECTS = 2, CategoryId = 6, Semester = 5 },
            new() { CourseCode = "BÝL324", CourseName = "MÝKROÝÞLEMCÝLER", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 5, CategoryId = 6, Semester = 5 },
    new() { CourseCode = "BÝL343", CourseName = "NESNE YÖNELÝMLÝ PROGRAMLAMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 6, Semester = 5 },
            new() { CourseCode = "BÝL367", CourseName = "ALGORÝTMALAR", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 6, Semester = 5 },
   new() { CourseCode = "ENG330", CourseName = "DEVELOPING ENGLISH LANGUAGE SKILLS", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = 6, Semester = 5 },
    new() { CourseCode = "MAT311", CourseName = "SAYISAL ANALÝZ TEKNÝKLERÝ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 5, CategoryId = 6, Semester = 5 },
     new() { CourseCode = "SOS204", CourseName = "ÝÞLETME", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 4, CategoryId = 6, Semester = 5 },

       // Altýncý Yarýyýl (Bahar) - Semester 6
     new() { CourseCode = "BÝL001", CourseName = "TEKNÝK SEÇÝMLÝK I", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 7, Semester = 6, IsElective = true },
    new() { CourseCode = "BÝL007", CourseName = "SOSYAL SEÇÝMLÝK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = 7, Semester = 6, IsElective = true },
            new() { CourseCode = "BÝL332", CourseName = "ÝÞLETÝM SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 7, CategoryId = 7, Semester = 6 },
       new() { CourseCode = "BÝL344", CourseName = "VERÝTABANI SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = 7, Semester = 6 },
            new() { CourseCode = "BÝL386", CourseName = "YAZILIM MÜHENDÝSLÝÐÝNE GÝRÝÞ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = 7, Semester = 6 },
            new() { CourseCode = "GSBHSH", CourseName = "SEÇMELÝ GÜZEL SANATLAR/ÝLK YARDIM", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 7, Semester = 6, IsElective = true },

    // Yedinci Yarýyýl (Güz) - Semester 7
  new() { CourseCode = "BÝL002", CourseName = "TEKNÝK SEÇÝMLÝK II", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 8, Semester = 7, IsElective = true },
         new() { CourseCode = "BÝL003", CourseName = "TEKNÝK SEÇÝMLÝK III", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 8, Semester = 7, IsElective = true },
            new() { CourseCode = "BÝL493", CourseName = "BÝTÝRME PROJESÝ I", TheoryHours = 0, PracticeHours = 4, Credits = 2, ECTS = 7, CategoryId = 8, Semester = 7 },
      new() { CourseCode = "BÝL498", CourseName = "STAJ II", TheoryHours = 0, PracticeHours = 0, Credits = 0, ECTS = 3, CategoryId = 8, Semester = 7 },
       new() { CourseCode = "BÝL499", CourseName = "BÝLGÝSAYAR AÐLARI", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 6, CategoryId = 8, Semester = 7 },
  new() { CourseCode = "ENG460", CourseName = "PRESENTATION SKILLS", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = 8, Semester = 7 },

     // Sekizinci Yarýyýl (Bahar) - Semester 8
          new() { CourseCode = "BÝL004", CourseName = "TEKNÝK SEÇÝMLÝK IV", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 9, Semester = 8, IsElective = true },
    new() { CourseCode = "BÝL005", CourseName = "TEKNÝK SEÇÝMLÝK V", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 9, Semester = 8, IsElective = true },
            new() { CourseCode = "BÝL006", CourseName = "TEKNÝK SEÇÝMLÝK VI", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 9, Semester = 8, IsElective = true },
            new() { CourseCode = "BÝL482", CourseName = "ETÝK,TOPLUM VE MESLEK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 7, CategoryId = 9, Semester = 8 },
            new() { CourseCode = "BÝL494", CourseName = "BÝTÝRME PROJESÝ II", TheoryHours = 0, PracticeHours = 4, Credits = 2, ECTS = 8, CategoryId = 9, Semester = 8 },

            // Teknik Seçmeli Dersler
          new() { CourseCode = "BÝL321", CourseName = "HESAPLAMALI GRAFÝK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL328", CourseName = "OTOMATA TEORÝSÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
          new() { CourseCode = "BÝL345", CourseName = "SÝSTEM MÜHENDÝSLÝÐÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
    new() { CourseCode = "BÝL363", CourseName = "ÝNSAN BÝLGÝSAYAR ETKÝLEÞÝMÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
        new() { CourseCode = "BÝL383", CourseName = "YÖNETÝM BÝLÝÞÝM SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
         new() { CourseCode = "BÝL387", CourseName = "WEB TASARIMI VE UYGULAMALARI", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
         new() { CourseCode = "BÝL388", CourseName = "E-TÝCARET VE E-ÝÞ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL389", CourseName = "BÝLGÝSAYAR AÐ UYGULAMALARI", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
     new() { CourseCode = "BÝL390", CourseName = "YAZILIM KALÝTE YÖNETÝMÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
       new() { CourseCode = "BÝL391", CourseName = "MOBÝL UYGULAMA GELÝÞTÝRME", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL392", CourseName = "OYUN TEKNOLOJÝLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
    new() { CourseCode = "BÝL393", CourseName = "YÖNEYLEM ARAÞTIRMASI", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
  new() { CourseCode = "BÝL395", CourseName = "UYGULAMALI VERÝ ANALÝZÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
   new() { CourseCode = "BÝL396", CourseName = "BÝLGÝSAYAR BÝLÝMLERÝNDE GÜNCEL KONULAR", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL443", CourseName = "KRÝPTOGRAFÝ VE GÜVENLÝK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
      new() { CourseCode = "BÝL447", CourseName = "GÖMÜLÜ SÝSTEMLER", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
        new() { CourseCode = "BÝL454", CourseName = "UYGULAMALI UML", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
   new() { CourseCode = "BÝL456", CourseName = "ÝMGE ÝÞLEME", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL457", CourseName = "KUANTUM HESAPLAMAYA GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
        new() { CourseCode = "BÝL458", CourseName = "BULUT ÇÖZÜME GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL459", CourseName = "YAZILIM MÝMARÝLERÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL466", CourseName = "BÝYOBÝLÝÞÝM", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
       new() { CourseCode = "BÝL471", CourseName = "BÝLGÝSAYARLA GÖRME", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL473", CourseName = "TIP BÝLÝÞÝMÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL475", CourseName = "ÇÝZGE KURAMI", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL477", CourseName = "VERÝ MADENCÝLÝÐÝNE GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
  new() { CourseCode = "BÝL478", CourseName = "PARALEL VERÝ ÝÞLEME", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
   new() { CourseCode = "BÝL479", CourseName = "ÖRÜNTÜ TANIMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL480", CourseName = "YAPAY ZEKA", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
       new() { CourseCode = "BÝL481", CourseName = "BÝLGÝSAYAR MÜHENDÝSLÝÐÝNDE ÖZEL KONULAR", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL489", CourseName = "ÇOKLUORTAM SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
            new() { CourseCode = "BÝL490", CourseName = "SAYISAL YÖNTEMLER VE OPTÝMÝZASYON", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
          new() { CourseCode = "BÝL495", CourseName = "UNIX SÝSTEM PROGRAMLAMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },
      new() { CourseCode = "BÝL497", CourseName = "GERÇEK ZAMANLI SÝSTEMLER", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true },

            // Sosyal Seçmeli Dersler
    new() { CourseCode = "SOS321", CourseName = "ÝLETÝÞÝM", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = 11, IsElective = true },
     new() { CourseCode = "SOS322", CourseName = "ÝÞLETME YÖNETÝMÝNE GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = 11, IsElective = true },

     // Ortak Seçmeli Dersler
            new() { CourseCode = "GSB101", CourseName = "FOTOÐRAFÇILIK", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
      new() { CourseCode = "GSB102", CourseName = "FOTOÐRAFÇILIK", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
     new() { CourseCode = "GSB103", CourseName = "HEYKEL", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
        new() { CourseCode = "GSB104", CourseName = "HEYKEL", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
  new() { CourseCode = "GSB105", CourseName = "KLASÝK MÜZÝK DÝNLEME KÜLTÜRÜ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
         new() { CourseCode = "GSB107", CourseName = "GÖRSEL KÜLTÜR VE SANATIN TARÝHÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
  new() { CourseCode = "GSB109", CourseName = "ANADOLU ARKEOLOJÝSÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
            new() { CourseCode = "GSB111", CourseName = "SÝNEMA KÜLTÜRÜ VE TARÝHÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
            new() { CourseCode = "GSB113", CourseName = "RESÝM", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
  new() { CourseCode = "GSB115", CourseName = "DOÐAÇLAMA VE ÝLÝÞKÝLÝ DOÐAÇLAMA", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
          new() { CourseCode = "GSB117", CourseName = "ÇAÐDAÞ DANSI ANLAMA VE YORUMLAMA", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
new() { CourseCode = "GSB119", CourseName = "ÝNSAN VE ÇEVRE ETKÝLEÞÝMÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
   new() { CourseCode = "GSB121", CourseName = "TASARIM KÜLTÜR VE TÜKETÝM", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
    new() { CourseCode = "GSB123", CourseName = "ÇAÐDAÞ DANSA GÝRÝÞ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
            new() { CourseCode = "GSB125", CourseName = "TAKI TASARIMI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
     new() { CourseCode = "GSB127", CourseName = "SERAMÝK", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
   new() { CourseCode = "GSB129", CourseName = "KENTLER VE TARÝHSEL ÇEVRE", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
   new() { CourseCode = "GSB131", CourseName = "21.YÜZYILDA DÜNYA VE SANATTA EÐÝLÝMLER", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
            new() { CourseCode = "GSB133", CourseName = "ÇAÐLAR BOYU MÜZÝK TÜRLERÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
  new() { CourseCode = "GSB135", CourseName = "SANAT VE EDEBÝYAT ESERLERÝNDE EVRENSEL HUKUK ÝLKELERÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
         new() { CourseCode = "GSB137", CourseName = "ETKÝLÝ VE GÜZEL KONUÞMA", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
 new() { CourseCode = "GSB139", CourseName = "GÜNCEL TEMEL EKONOMÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
   new() { CourseCode = "HSH100", CourseName = "TEMEL ÝLK YARDIM", TheoryHours = 1, PracticeHours = 1, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
            new() { CourseCode = "KAM100", CourseName = "KIBRIS TÜRKLERÝNÝN YAKIN TARÝHÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
         new() { CourseCode = "TIP099", CourseName = "TOPLUMSAL CÝNSÝYET VE KADINA YÖNELÝK ÞÝDDET", TheoryHours = 1, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },
        new() { CourseCode = "YAKE100", CourseName = "YARATICI KÜLTÜR ENDÜSTRÝLERÝ", TheoryHours = 2, PracticeHours = 0, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true },

            // Katalog Dýþý Seçmeli Ders
            new() { CourseCode = "GNLÇ310", CourseName = "GÖNÜLLÜLÜK ÇALIÞMALARI", TheoryHours = 1, PracticeHours = 2, Credits = 2, ECTS = 4, CategoryId = 13, IsElective = true }
        };

        await db.Courses.AddRangeAsync(courses);
 await db.SaveChangesAsync();

     // Önkoþul iliþkilerini ekle
        await AddPrerequisites(db);
    }

    private static async Task AddPrerequisites(AppDbContext db)
    {
    var bil122 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL122");
        var bil101 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL101");
        
        var mat152 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT152");
        var mat151 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT151");
  
   var bil265 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL265");
        
        var eng200 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "ENG200");
        var eng199 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "ENG199");
        
        var bil210 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL210");
        var fiz110 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "FÝZ110");
        
var bil218 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL218");
        var bil275 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL275");
    
var bil240 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL240");
        
        var mat286 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT286");
        
        var bil367 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL367");
        
 var mat311 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "MAT311");
   
        var bil390 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL390");
  var bil386 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL386");
  
        var bil498 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL498");
    var bil300 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL300");
        
        var bil494 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL494");
        var bil493 = await db.Courses.FirstOrDefaultAsync(c => c.CourseCode == "BÝL493");

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
