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

      // Mevcut veriyi temizle ve yeniden seed et (collation düzeltmesi için)
 var existingCourses = await db.Courses.ToListAsync();
        if (existingCourses.Any())
 {
            // Önce baðýmlý tablolarý temizle
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
         new() { CourseCode = "KRY100", CourseName = "KARÝYER PLANLAMA", TheoryHours = 1, PracticeHours = 0, Credits = 1, ECTS = 2, CategoryId = 1, Description = "Kariyer hedeflerini belirleme ve profesyonel geliþim stratejileri üzerine odaklanan bir derstir." },
            new() { CourseCode = "ORY100", CourseName = "ÜNÝVERSÝTE HAYATINA GÝRÝÞ", TheoryHours = 1, PracticeHours = 0, Credits = 1, ECTS = 1, CategoryId = 1, Description = "Üniversite yaþamýna uyum, akademik beceriler ve kampüs kaynaklarýnýn tanýtýmýný içerir." },

            // Birinci Yarýyýl (Güz) - Semester 1
  new() { CourseCode = "BÝL101", CourseName = "BÝLGÝSAYAR YAZILIMI I", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 2, Semester = 1, Description = "Programlama temellerini ve yazýlým geliþtirme süreçlerinin ilk adýmlarýný öðretir." },
  new() { CourseCode = "BÝL105", CourseName = "PROGRAMLAMA LABORATUVARI I", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 2, Semester = 1, Description = "Temel programlama kavramlarýnýn uygulamalý olarak pekiþtirildiði laboratuvar dersidir." },
          new() { CourseCode = "BÝL110", CourseName = "BÝLGÝSAYAR MÜHENDÝSLÝÐÝNE GÝRÝÞ", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 4, CategoryId = 2, Semester = 1, Description = "Bilgisayar mühendisliði disiplininin genel tanýtýmý ve temel kavramlarýnýn öðretildiði derstir." },
new() { CourseCode = "ENG199", CourseName = "ADVANCED ENGLISH I", TheoryHours = 4, PracticeHours = 0, Credits = 4, ECTS = 4, CategoryId = 2, Semester = 1, Description = "Ýleri seviye Ýngilizce dil becerileri geliþtirme ve akademik Ýngilizce yazma dersidir." },
new() { CourseCode = "FÝZ103", CourseName = "MEKANÝK LABORATUVARI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 2, Semester = 1, Description = "Temel mekanik prensiplerinin deneysel olarak incelendiði laboratuvar çalýþmasýdýr." },
  new() { CourseCode = "FÝZ105", CourseName = "GENEL FÝZÝK I", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 2, Semester = 1, Description = "Klasik mekanik, hareket yasalarý ve enerji kavramlarýný kapsayan temel fizik dersidir." },
            new() { CourseCode = "MAT151", CourseName = "MATEMATÝKSEL ANALÝZ I", TheoryHours = 4, PracticeHours = 1, Credits = 4, ECTS = 6, CategoryId = 2, Semester = 1, Description = "Limit, süreklilik, türev ve integral konularýný içeren temel matematik dersidir." },
       new() { CourseCode = "TÜRK101", CourseName = "TÜRK DÝLÝ I", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 2, Semester = 1, Description = "Türkçe dil kurallarý, akademik yazma ve etkili iletiþim becerilerinin geliþtirildiði derstir." },

    // Ýkinci Yarýyýl (Bahar) - Semester 2
            new() { CourseCode = "BÝL122", CourseName = "ÝLERÝ PROGRAMLAMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 3, Semester = 2, Description = "Nesne yönelimli programlama ve ileri düzey yazýlým geliþtirme tekniklerini öðretir." },
 new() { CourseCode = "BÝL124", CourseName = "ÝLERÝ PROGRAMLAMA UYGULAMALARI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 3, Semester = 2, Description = "Ýleri programlama kavramlarýnýn pratik uygulamalarla pekiþtirildiði laboratuvar dersidir." },
          new() { CourseCode = "BÝL172", CourseName = "YAÞAM BÝLÝMLERÝ VE BÝLGÝSAYAR MÜHENDÝSLÝÐÝ", TheoryHours = 2, PracticeHours = 1, Credits = 2, ECTS = 4, CategoryId = 3, Semester = 2, Description = "Biyoinformatik ve hesaplamalý biyoloji alanlarýnda bilgisayar mühendisliðinin uygulamalarýný inceler." },
            new() { CourseCode = "FÝZ104", CourseName = "ELEKTRÝK LABORATUVARI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 2, CategoryId = 3, Semester = 2, Description = "Temel elektrik ve manyetizma prensipler deneysel olarak incelindiði laboratuvar çalýþmasýdýr." },
        new() { CourseCode = "FÝZ110", CourseName = "GENEL FÝZÝK II", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 3, Semester = 2, Description = "Elektromanyetizma, optik ve modern fizik konularýný kapsayan temel fizik dersidir." },
    new() { CourseCode = "MAT152", CourseName = "MATEMATÝKSEL ANALÝZ II", TheoryHours = 4, PracticeHours = 1, Credits = 4, ECTS = 6, CategoryId = 3, Semester = 2, Description = "Çok deðiþkenli fonksiyonlar, çift integraller ve seri konularýný içeren ileri matematik dersidir." },
            new() { CourseCode = "MAT210", CourseName = "DOÐRUSAL CEBÝR", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = 3, Semester = 2, Description = "Matrisler, vektör uzaylarý ve doðrusal dönüþümler konularýný kapsayan temel matematik dersidir." },
    new() { CourseCode = "TÜRK102", CourseName = "TÜRK DÝLÝ II", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 3, Semester = 2, Description = "Ýleri seviye Türkçe yazýlý ve sözlü iletiþim becerilerinin geliþtirildiði derstir." },

       // Üçüncü Yarýyýl (Güz) - Semester 3
    new() { CourseCode = "ATA201", CourseName = "ATATÜRK ÝLKELERÝ VE ÝNKILAP TARÝHÝ I", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 4, Semester = 3, Description = "Atatürk ilkeleri ve Türkiye Cumhuriyeti'nin kuruluþ sürecinin incelendiði tarih dersidir." },
     new() { CourseCode = "BÝL231", CourseName = "AYRIK YAPILAR", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = 4, Semester = 3, Description = "Kümeler, mantýk, sayma teknikleri ve graflar gibi ayrýk matematik konularýný kapsar." },
            new() { CourseCode = "BÝL265", CourseName = "VERÝ YAPILARI", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 7, CategoryId = 4, Semester = 3, Description = "Diziler, listeler, aðaçlar ve hash tablolarý gibi temel veri yapýlarýný öðretir." },
       new() { CourseCode = "BÝL275", CourseName = "SAYISAL MANTIK TASARIMI", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = 4, Semester = 3, Description = "Boolean cebiri, kombinasyonel ve ardýþýl mantýk devrelerinin tasarýmýný içerir." },
          new() { CourseCode = "ENG200", CourseName = "ADVANCED ENGLISH II", TheoryHours = 4, PracticeHours = 0, Credits = 4, ECTS = 4, CategoryId = 4, Semester = 3, Description = "Akademik Ýngilizce ve profesyonel iletiþim becerilerinin ileri düzeyde geliþtirildiði derstir." },
            new() { CourseCode = "SOS203", CourseName = "EKONOMÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 4, CategoryId = 4, Semester = 3, Description = "Mikro ve makro ekonomi temellerini ve ekonomik sistemlerin iþleyiþini inceleyen derstir." },

          // Dördüncü Yarýyýl (Bahar) - Semester 4
            new() { CourseCode = "ATA202", CourseName = "ATATÜRK ÝLKELERÝ VE ÝNKILAP TARÝHÝ II", TheoryHours = 2, PracticeHours = 0, Credits = 2, ECTS = 2, CategoryId = 5, Semester = 4, Description = "Türkiye Cumhuriyeti'nin modernleþme süreci ve çaðdaþ geliþmelerin incelendiði tarih dersidir." },
            new() { CourseCode = "BÝL210", CourseName = "ELEKTRONÝÐE GÝRÝÞ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 6, CategoryId = 5, Semester = 4, Description = "Temel elektronik devre elemanlarý ve yarý iletken cihazlarýn çalýþma prensiplerini öðretir." },
   new() { CourseCode = "BÝL218", CourseName = "BÝLGÝSAYAR ORGANÝZASYONU", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = 5, Semester = 4, Description = "Bilgisayar mimarisi, iþlemci tasarýmý ve bellek organizasyonu konularýný kapsar." },
          new() { CourseCode = "BÝL240", CourseName = "PROGRAMLAMA DÝLLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 6, CategoryId = 5, Semester = 4, Description = "Farklý programlama paradigmalarý ve dil tasarým prensiplerinin karþýlaþtýrmalý incelemesini yapar." },
new() { CourseCode = "MAT250", CourseName = "OLASILIK VE ÝSTATÝSTÝK", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 5, Semester = 4, Description = "Olasýlýk teorisi, istatistiksel analiz ve veri yorumlama tekniklerini öðretir." },
    new() { CourseCode = "MAT286", CourseName = "BÝLGÝSAYAR MÜHENDÝSLÝÐÝ ÝÇÝN MATEMATÝK", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 5, Semester = 4, Description = "Bilgisayar mühendisliðinde kullanýlan ileri matematik konularýný ve uygulamalarýný kapsar." },

            // Beþinci Yarýyýl (Güz) - Semester 5
 new() { CourseCode = "BÝL300", CourseName = "STAJ I", TheoryHours = 0, PracticeHours = 0, Credits = 0, ECTS = 2, CategoryId = 6, Semester = 5, Description = "Öðrencilerin sektörde pratik deneyim kazanmalarýný saðlayan ilk staj dönemidir." },
            new() { CourseCode = "BÝL324", CourseName = "MÝKROÝÞLEMCÝLER", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 5, CategoryId = 6, Semester = 5, Description = "Mikroiþlemci mimarisi, assembly programlama ve gömülü sistem tasarýmý konularýný içerir." },
            new() { CourseCode = "BÝL343", CourseName = "NESNE YÖNELÝMLÝ PROGRAMLAMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 6, Semester = 5, Description = "Nesne yönelimli tasarým prensipleri ve C++ veya Java dilinde ileri düzey programlamayý öðretir." },
      new() { CourseCode = "BÝL367", CourseName = "ALGORÝTMALAR", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 6, Semester = 5, Description = "Algoritma tasarýmý, karmaþýklýk analizi ve optimizasyon tekniklerini detaylý olarak inceler." },
          new() { CourseCode = "ENG330", CourseName = "DEVELOPING ENGLISH LANGUAGE SKILLS", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = 6, Semester = 5, Description = "Profesyonel ortamlarda etkili Ýngilizce iletiþim ve sunum becerilerinin geliþtirildiði derstir." },
            new() { CourseCode = "MAT311", CourseName = "SAYISAL ANALÝZ TEKNÝKLERÝ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 5, CategoryId = 6, Semester = 5, Description = "Sayýsal çözüm yöntemleri, hata analizi ve bilgisayarlý hesaplama tekniklerini öðretir." },
     new() { CourseCode = "SOS204", CourseName = "ÝÞLETME", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 4, CategoryId = 6, Semester = 5, Description = "Ýþletme yönetimi, organizasyon ve temel ticari kavramlarýn öðretildiði derstir." },

    // Altýncý Yarýyýl (Bahar) - Semester 6
 new() { CourseCode = "BÝL001", CourseName = "TEKNÝK SEÇMELÝK I", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 7, Semester = 6, IsElective = true, Description = "Öðrencinin ilgi alanýna göre seçtiði teknik konularda derinlemesine bilgi edindiði seçmeli derstir." },
   new() { CourseCode = "BÝL007", CourseName = "SOSYAL SEÇMELÝK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = 7, Semester = 6, IsElective = true, Description = "Sosyal bilimler alanýnda geniþ bir perspektif kazandýrmayý amaçlayan seçmeli derstir." },
            new() { CourseCode = "BÝL332", CourseName = "ÝÞLETÝM SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 7, CategoryId = 7, Semester = 6, Description = "Ýþletim sistemi mimarisi, süreç yönetimi ve kaynak tahsisi konularýný detaylý inceler." },
  new() { CourseCode = "BÝL344", CourseName = "VERÝTABANI SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = 7, Semester = 6, Description = "Ýliþkisel veritabaný tasarýmý, SQL programlama ve veritabaný yönetim sistemlerini öðretir." },
         new() { CourseCode = "BÝL386", CourseName = "YAZILIM MÜHENDÝSLÝÐÝNE GÝRÝÞ", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 7, CategoryId = 7, Semester = 6, Description = "Yazýlým geliþtirme yaþam döngüsü, proje yönetimi ve kalite güvence süreçlerini kapsar." },
       new() { CourseCode = "GSBHSH", CourseName = "SEÇMELÝ GÜZEL SANATLAR/ÝLK YARDIM", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 7, Semester = 6, IsElective = true, Description = "Güzel sanatlar veya temel ilk yardým bilgisi edinilen seçmeli uygulamalý derstir." },

  // Yedinci Yarýyýl (Güz) - Semester 7
 new() { CourseCode = "BÝL002", CourseName = "TEKNÝK SEÇMELÝK II", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 8, Semester = 7, IsElective = true, Description = "Öðrencinin uzmanlýk alanýnda ileri düzey teknik bilgi edindiði ikinci seçmeli derstir." },
        new() { CourseCode = "BÝL003", CourseName = "TEKNÝK SEÇMELÝK III", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 8, Semester = 7, IsElective = true, Description = "Öðrencinin kariyer hedeflerine yönelik özel teknik konularda uzmanlaþtýðý üçüncü seçmeli derstir." },
      new() { CourseCode = "BÝL493", CourseName = "BÝTÝRME PROJESÝ I", TheoryHours = 0, PracticeHours = 4, Credits = 2, ECTS = 7, CategoryId = 8, Semester = 7, Description = "Öðrencinin danýþman gözetiminde bir araþtýrma veya uygulama projesi baþlattýðý ilk dönemdir." },
   new() { CourseCode = "BÝL498", CourseName = "STAJ II", TheoryHours = 0, PracticeHours = 0, Credits = 0, ECTS = 3, CategoryId = 8, Semester = 7, Description = "Öðrencilerin sektörde ileri düzey pratik deneyim kazandýklarý ikinci staj dönemidir." },
     new() { CourseCode = "BÝL499", CourseName = "BÝLGÝSAYAR AÐLARI", TheoryHours = 3, PracticeHours = 2, Credits = 4, ECTS = 6, CategoryId = 8, Semester = 7, Description = "Að protokolleri, TCP/IP, routing ve að güvenliði konularýný kapsamlý olarak inceler." },
     new() { CourseCode = "ENG460", CourseName = "PRESENTATION SKILLS", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 4, CategoryId = 8, Semester = 7, Description = "Profesyonel sunum hazýrlama ve etkili iletiþim tekniklerinin geliþtirildiði Ýngilizce derstir." },

            // Sekizinci Yarýyýl (Bahar) - Semester 8
   new() { CourseCode = "BÝL004", CourseName = "TEKNÝK SEÇMELÝK IV", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 9, Semester = 8, IsElective = true, Description = "Öðrencinin mezuniyet öncesi son teknik derinleþme fýrsatý sunduðu dördüncü seçmeli derstir." },
new() { CourseCode = "BÝL005", CourseName = "TEKNÝK SEÇMELÝK V", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 9, Semester = 8, IsElective = true, Description = "Öðrencinin ilgi alanýndaki güncel teknolojileri öðrendiði beþinci seçmeli derstir." },
    new() { CourseCode = "BÝL006", CourseName = "TEKNÝK SEÇMELÝK VI", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 9, Semester = 8, IsElective = true, Description = "Öðrencinin mezuniyet öncesi ek uzmanlýk kazandýðý altýncý seçmeli derstir." },
    new() { CourseCode = "BÝL482", CourseName = "ETÝK,TOPLUM VE MESLEK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 7, CategoryId = 9, Semester = 8, Description = "Mühendislik etiði, mesleki sorumluluklar ve toplumsal etki konularýný inceleyen derstir." },
            new() { CourseCode = "BÝL494", CourseName = "BÝTÝRME PROJESÝ II", TheoryHours = 0, PracticeHours = 4, Credits = 2, ECTS = 8, CategoryId = 9, Semester = 8, Description = "Bitirme projesinin tamamlandýðý ve sonuçlarýn sunulduðu final dönemidir." },

      // Teknik Seçmeli Dersler
       new() { CourseCode = "BÝL321", CourseName = "HESAPLAMALI GRAFÝK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "2D ve 3D grafik algoritmalarý, görüntü iþleme ve bilgisayar animasyonu tekniklerini öðretir." },
            new() { CourseCode = "BÝL328", CourseName = "OTOMATA TEORÝSÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Biçimsel diller, otomatlar ve hesaplanabilirlik teorisinin temellerini inceler." },
            new() { CourseCode = "BÝL345", CourseName = "SÝSTEM MÜHENDÝSLÝÐÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Büyük ölçekli sistemlerin tasarýmý, entegrasyonu ve yönetimi konularýný kapsar." },
       new() { CourseCode = "BÝL363", CourseName = "ÝNSAN BÝLGÝSAYAR ETKÝLEÞÝMÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Kullanýcý arayüzü tasarýmý, kullanýlabilirlik ve etkileþim teknolojilerini inceler." },
  new() { CourseCode = "BÝL383", CourseName = "YÖNETÝM BÝLÝÞÝM SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Kurumsal bilgi sistemleri, iþ zekasý ve karar destek sistemlerini öðretir." },
            new() { CourseCode = "BÝL387", CourseName = "WEB TASARIMI VE UYGULAMALARI", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Modern web teknolojileri, responsive tasarým ve web uygulamasý geliþtirme tekniklerini kapsar." },
            new() { CourseCode = "BÝL388", CourseName = "E-TÝCARET VE E-ÝÞ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Elektronik ticaret modelleri, online iþ süreçleri ve dijital pazarlama stratejilerini inceler." },
            new() { CourseCode = "BÝL389", CourseName = "BÝLGÝSAYAR AÐ UYGULAMALARI", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Að programlama, socket iletiþimi ve daðýtýk sistem uygulamalarýnýn geliþtirilmesini öðretir." },
      new() { CourseCode = "BÝL390", CourseName = "YAZILIM KALÝTE YÖNETÝMÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Yazýlým kalite standartlarý, test metodolojileri ve süreç iyileþtirme tekniklerini kapsar." },
            new() { CourseCode = "BÝL391", CourseName = "MOBÝL UYGULAMA GELÝÞTÝRME", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "iOS ve Android platformlarý için mobil uygulama tasarýmý ve geliþtirme süreçlerini öðretir." },
  new() { CourseCode = "BÝL392", CourseName = "OYUN TEKNOLOJÝLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Oyun motoru kullanýmý, 2D/3D oyun geliþtirme ve oyun tasarýmý prensiplerini kapsar." },
            new() { CourseCode = "BÝL393", CourseName = "YÖNEYLEMsARAÞTIRMASI", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Matematiksel optimizasyon, doðrusal programlama ve karar analizi tekniklerini öðretir." },
            new() { CourseCode = "BÝL395", CourseName = "UYGULAMALI VERÝ ANALÝZÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Veri madenciliði, istatistiksel analiz ve veri görselleþtirme tekniklerinin uygulamalý öðretimini yapar." },
            new() { CourseCode = "BÝL396", CourseName = "BÝLGÝSAYAR BÝLÝMLERÝNDE GÜNCEL KONULAR", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Bilgisayar bilimlerindeki en yeni trendler ve geliþmekte olan teknolojilerin incelendiði derstir." },
            new() { CourseCode = "BÝL443", CourseName = "KRÝPTOGRAFÝ VE GÜVENLÝK", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Þifreleme algoritmalarý, að güvenliði ve bilgi güvenliði prensiplerini detaylý inceler." },
          new() { CourseCode = "BÝL447", CourseName = "GÖMÜLÜSÝSTEMLER", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Gömülü sistem tasarýmý, mikrodenetleyici programlama ve IoT uygulamalarýný öðretir." },
            new() { CourseCode = "BÝL454", CourseName = "UYGULAMALI UML", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Unified Modeling Language kullanarak yazýlým modelleme ve tasarým dokümantasyonu yapýlmasýný öðretir." },
new() { CourseCode = "BÝL456", CourseName = "ÝMGE ÝÞLEME", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Dijital görüntü iþleme algoritmalarý, filtreleme ve görüntü analizi tekniklerini kapsar." },
 new() { CourseCode = "BÝL457", CourseName = "KUANTUM HESAPLAMAYA GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Kuantum hesaplama prensipleri, kuantum algoritmalarý ve gelecekteki uygulamalarý inceler." },
            new() { CourseCode = "BÝL458", CourseName = "BULUT ÇÖZÜME GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Cloud computing mimarisi, servis modelleri ve bulut tabanlý uygulama geliþtirmeyi öðretir." },
      new() { CourseCode = "BÝL459", CourseName = "YAZILIM MÝMARÝLERÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Yazýlým mimari paternleri, mikroservis mimarisi ve sistem tasarým prensiplerini detaylý inceler." },
            new() { CourseCode = "BÝL466", CourseName = "BÝYOBÝLÝÞÝM", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Biyoinformatik algoritmalarý, genomik veri analizi ve hesaplamalý biyoloji uygulamalarýný kapsar." },
  new() { CourseCode = "BÝL471", CourseName = "BÝLGÝSAYARLA GÖRME", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Görüntü tanýma, nesne tespiti ve bilgisayarlý görü algoritmalarý öðretilir." },
  new() { CourseCode = "BÝL473", CourseName = "TIP BÝLÝÞÝMÝ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Týbbi veri yönetimi, saðlýk biliþim sistemleri ve týbbi görüntü iþleme konularýný inceler." },
            new() { CourseCode = "BÝL475", CourseName = "ÇÝZGE KURAMI", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Graflar, að akýþý, çizge algoritmalarý ve optimizasyon problemlerinin çözümünü öðretir." },
            new() { CourseCode = "BÝL477", CourseName = "VERÝ MADENCÝLÝÐÝNE GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Büyük veri setlerinden bilgi çýkarma, kümeleme ve sýnýflandýrma tekniklerini kapsar." },
    new() { CourseCode = "BÝL478", CourseName = "PARALEL VERÝ ÝÞLEME", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Paralel programlama teknikleri, çok çekirdekli iþlemciler ve daðýtýk hesaplama sistemlerini inceler." },
            new() { CourseCode = "BÝL479", CourseName = "ÖRÜNTÜ TANIMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Makine öðrenmesi tabanlý örüntü tanýma, özellik çýkarma ve sýnýflandýrma yöntemlerini öðretir." },
 new() { CourseCode = "BÝL480", CourseName = "YAPAY ZEKA", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Yapay zeka teknikleri, makine öðrenmesi algoritmalarý ve akýllý sistemlerin tasarýmýný kapsar." },
       new() { CourseCode = "BÝL481", CourseName = "BÝLGÝSAYAR MÜHENDÝSLÝÐÝNDE ÖZEL KONULAR", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Bilgisayar mühendisliðinde ileri düzey ve özel araþtýrma konularýnýn ele alýndýðý derstir." },
        new() { CourseCode = "BÝL489", CourseName = "ÇOKLUORTAM SÝSTEMLERÝ", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Multimedya veri iþleme, sýkýþtýrma teknikleri ve etkileþimli medya uygulamalarýný öðretir." },
            new() { CourseCode = "BÝL490", CourseName = "SAYISAL YÖNTEMLER VE OPTÝMÝZASYON", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Sayýsal çözüm teknikleri, optimizasyon algoritmalarý ve mühendislik problemlerine uygulanmasýný kapsar." },
       new() { CourseCode = "BÝL495", CourseName = "UNIX SÝSTEM PROGRAMLAMA", TheoryHours = 3, PracticeHours = 1, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Unix/Linux sistem çaðrýlarý, shell programlama ve sistem seviyesi yazýlým geliþtirmeyi öðretir." },
      new() { CourseCode = "BÝL497", CourseName = "GERÇEK ZAMANLI SÝSTEMLER", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 5, CategoryId = 10, IsElective = true, Description = "Gerçek zamanlý iþletim sistemleri, zamanlama algoritmalarý ve kritik sistem tasarýmý konularýný inceler." },

     // Sosyal Seçmeli Dersler
       new() { CourseCode = "SOS321", CourseName = "ÝLETÝÞÝM", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = 11, IsElective = true, Description = "Etkili iletiþim teknikleri, kiþilerarasý iliþkiler ve profesyonel iletiþim becerilerinin geliþtirildiði derstir." },
  new() { CourseCode = "SOS322", CourseName = "ÝÞLETME YÖNETÝMÝNE GÝRÝÞ", TheoryHours = 3, PracticeHours = 0, Credits = 3, ECTS = 3, CategoryId = 11, IsElective = true, Description = "Temel yönetim fonksiyonlarý, organizasyon yapýlarý ve liderlik kavramlarýnýn öðretildiði derstir." },

            // Ortak Seçmeli Dersler
            new() { CourseCode = "GSB101", CourseName = "FOTOÐRAFÇILIK", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Temel fotoðraf teknikleri, kompozisyon ve dijital fotoðrafçýlýk uygulamalarýnýn öðretildiði derstir." },
  new() { CourseCode = "GSB102", CourseName = "FOTOÐRAFÇILIK", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Ýleri fotoðraf teknikleri ve sanatsal fotoðrafçýlýk uygulamalarýnýn geliþtirildiði derstir." },
         new() { CourseCode = "GSB103", CourseName = "HEYKEL", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Temel heykel yapým teknikleri ve üç boyutlu sanat eserlerinin oluþturulduðu uygulamalý derstir." },
     new() { CourseCode = "GSB104", CourseName = "HEYKEL", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Ýleri heykel teknikleri ve modern heykel sanatý uygulamalarýnýn öðretildiði derstir." },
     new() { CourseCode = "GSB105", CourseName = "KLASÝK MÜZÝK DÝNLEME KÜLTÜRÜ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Klasik müzik tarihi, beste analizi ve müzik dinleme becerilerinin geliþtirildiði derstir." },
  new() { CourseCode = "GSB107", CourseName = "GÖRSEL KÜLTÜR VE SANATIN TARÝHÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Sanat tarihi, görsel kültür ve sanatsal akýmlarýn kronolojik incelendiði derstir." },
            new() { CourseCode = "GSB109", CourseName = "ANADOLU ARKEOLOJÝSÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Anadolu'nun arkeolojik zenginlikleri ve antik medeniyetlerin incelendiði derstir." },
            new() { CourseCode = "GSB111", CourseName = "SÝNEMA KÜLTÜRÜ VE TARÝHÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Sinema sanatýnýn geliþimi, film analizi ve görsel anlatým tekniklerinin öðretildiði derstir." },
          new() { CourseCode = "GSB113", CourseName = "RESÝM", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Temel resim teknikleri, renk teorisi ve görsel kompozisyon uygulamalarýnýn yapýldýðý derstir." },
  new() { CourseCode = "GSB115", CourseName = "DOÐAÇLAMA VE ÝLÝÞKÝLÝ DOÐAÇLAMA", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Tiyatro doðaçlama teknikleri, yaratýcýlýk ve spontan performans becerilerinin geliþtirildiði derstir." },
       new() { CourseCode = "GSB117", CourseName = "ÇAÐDAÞ DANSI ANLAMA VE YORUMLAMA", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Modern dans teknikleri, koreografi analizi ve dans performansýnýn öðretildiði uygulamalý derstir." },
         new() { CourseCode = "GSB119", CourseName = "ÝNSAN VE ÇEVRE ETKÝLEÞÝMÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Çevre sorunlarý, sürdürülebilirlik ve insan-doða iliþkisinin incelendiði derstir." },
            new() { CourseCode = "GSB121", CourseName = "TASARIM KÜLTÜR VE TÜKETÝM", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Tasarým felsefesi, tüketim kültürü ve modern tasarýmýn toplumsal etkilerinin analiz edildiði derstir." },
      new() { CourseCode = "GSB123", CourseName = "ÇAÐDAÞ DANSA GÝRÝÞ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Çaðdaþ dans temel hareketleri ve beden farkýndalýðý çalýþmalarýnýn yapýldýðý uygulamalý derstir." },
    new() { CourseCode = "GSB125", CourseName = "TAKI TASARIMI", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Taký tasarýmý prensipleri ve el yapýmý taký üretim tekniklerinin öðretildiði uygulamalý derstir." },
            new() { CourseCode = "GSB127", CourseName = "SERAMÝK", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Seramik þekillendirme, sýrlama teknikleri ve fýrýnlama süreçlerinin uygulandýðý derstir." },
        new() { CourseCode = "GSB129", CourseName = "KENTLER VE TARÝHSEL ÇEVRE", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Kentsel geliþim, tarihi çevre koruma ve þehir planlamasýnýn incelendiði derstir." },
         new() { CourseCode = "GSB131", CourseName = "21.YÜZYILDA DÜNYA VE SANATTA EÐÝLÝMLER", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Çaðdaþ sanat akýmlarý, küresel sanat trendleri ve kültürel eðilimlerin analiz edildiði derstir." },
     new() { CourseCode = "GSB133", CourseName = "ÇAÐLAR BOYU MÜZÝK TÜRLERÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Müzik tarihinde farklý dönemler ve müzik türlerinin kronolojik incelendiði derstir." },
      new() { CourseCode = "GSB135", CourseName = "SANAT VE EDEBÝYAT ESERLERÝNDE EVRENSEL HUKUK ÝLKELERÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Edebiyat ve sanatta hukuki temalar ve evrensel adalet kavramlarýnýn incelendiði derstir." },
  new() { CourseCode = "GSB137", CourseName = "ETKÝLÝ VE GÜZEL KONUÞMA", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Hitabet sanatý, diksiyon ve etkili sunum becerilerinin geliþtirildiði uygulamalý derstir." },
          new() { CourseCode = "GSB139", CourseName = "GÜNCEL TEMEL EKONOMÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Güncel ekonomik olaylar, ekonomi politikalarý ve finansal okuryazarlýðýn öðretildiði derstir." },
         new() { CourseCode = "HSH100", CourseName = "TEMEL ÝLK YARDIM", TheoryHours = 1, PracticeHours = 1, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Acil durumlarda hayat kurtarýcý temel ilk yardým tekniklerinin uygulamalý öðretildiði derstir." },
            new() { CourseCode = "KAM100", CourseName = "KIBRIS TÜRKLERÝNÝN YAKIN TARÝHÝ", TheoryHours = 0, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Kýbrýs Türk toplumunun yakýn tarihi ve siyasi geliþmelerin incelendiði derstir." },
         new() { CourseCode = "TIP099", CourseName = "TOPLUMSAL CÝNSÝYET VE KADINA YÖNELÝK ÞÝDDET", TheoryHours = 1, PracticeHours = 2, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Toplumsal cinsiyet eþitliði, kadýn haklarý ve þiddetin önlenmesi konularýný inceler." },
            new() { CourseCode = "YAKE100", CourseName = "YARATICI KÜLTÜR ENDÜSTRÝLERÝ", TheoryHours = 2, PracticeHours = 0, Credits = 1, ECTS = 1, CategoryId = 12, IsElective = true, Description = "Yaratýcý endüstriler, kültürel ekonomi ve dijital içerik üretiminin incelendiði derstir." },

 // Katalog Dýþý Seçmeli Ders
          new() { CourseCode = "GNLÝ310", CourseName = "GÖNÜLLÜLÜK ÇALIÞMALARI", TheoryHours = 1, PracticeHours = 2, Credits = 2, ECTS = 4, CategoryId = 13, IsElective = true, Description = "Toplumsal sorumluluk projeleri ve gönüllülük faaliyetlerinin uygulamalý gerçekleþtirildiði derstir." }
        };

        await db.Courses.AddRangeAsync(courses);
        await db.SaveChangesAsync();

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
