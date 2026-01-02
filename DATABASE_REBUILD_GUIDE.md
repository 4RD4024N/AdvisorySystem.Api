# ?? Database Rebuild - Tam Rehber

**Durum:** Database'i sýfýrdan oluþturmak gerekiyor  
**Sebep:** Açýklamalar eksik, Türkçe karakterler bozuk veya migration sorunlarý

---

## ? HIZLI ÇÖZÜM (2 Dakika)

### Seçenek 1: Tam Otomatik (Önerilen)

```powershell
# PowerShell'de çalýþtýr
.\rebuild-database.ps1
```

**"EVET" yazýn ve bekleyin**

Ne yapar:
- ? Database'i siler
- ? Yeniden oluþturur
- ? Migration'larý uygular
- ? Seeding yapar (20 saniye)
- ? Sonuçlarý gösterir

---

### Seçenek 2: Hýzlý Manuel

```powershell
.\quick-rebuild.ps1
```

**"E" yazýn**

Sonra:
1. `dotnet run`
2. 20 saniye bekle
3. Ctrl+C

---

## ?? Manuel Yöntem (Adým Adým)

```powershell
# 1. Database'i sil
dotnet ef database drop --force

# Beklenen çýktý:
# Dropping database 'AdvisorySystemDB' on server '(localdb)\MSSQLLocalDB'.
# Successfully dropped database 'AdvisorySystemDB'.

# 2. Yeniden oluþtur
dotnet ef database update

# Beklenen çýktý:
# Applying migration '20241220_InitialCreate'.
# Applying migration '20241223_AddStudentProfileAndRatingFeatures'.
# ... (6 migration)
# Done.

# 3. Seeding için uygulamayý baþlat
dotnet run

# Beklenen çýktý (console'da):
# info: AdvisorySystem.Api.Services.IdentitySeeder[0]
#   Seeding identity data...
# info: AdvisorySystem.Api.Services.CourseSeeder[0]
#   Seeding courses...
# ? 13 categories seeded
# ? 117 courses seeded

# 4. 20 saniye sonra durdur
# Ctrl+C
```

---

## ?? Doðrulama

### Test 1: SQL ile Kontrol

```sql
-- Visual Studio SQL Server Object Explorer
-- AdvisorySystemDB -> New Query

-- Tablo sayýlarý
SELECT 'Users' as [Table], COUNT(*) as [Count] FROM AspNetUsers
UNION ALL SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL SELECT 'Categories', COUNT(*) FROM CourseCategories;

-- Beklenen:
-- Users: 7 (1 admin + 3 advisor + 3 student)
-- Courses: 117
-- Categories: 13
```

### Test 2: Açýklamalarý Kontrol

```sql
SELECT 
    COUNT(*) as Total,
    COUNT(Description) as WithDesc
FROM Courses;

-- Beklenen:
-- Total: 117
-- WithDesc: 117
```

### Test 3: Örnek Ders

```sql
SELECT TOP 1 
    CourseCode,
    CourseName,
    Description,
    LEN(Description) as DescLength
FROM Courses
WHERE CourseCode = 'BÝL101';

-- Beklenen:
-- BÝL101
-- BÝLGÝSAYAR YAZILIMI I
-- Programlama temellerini ve yazýlým geliþtirme süreçlerinin ilk adýmlarýný öðretir.
-- 82
```

### Test 4: API Diagnostics

```bash
# Uygulamayý baþlat
dotnet run

# Baþka bir terminalde veya tarayýcýda
curl https://localhost:44375/api/courses/diagnostics
```

Beklenen:
```json
{
  "summary": {
    "totalCourses": 117,
    "coursesWithDescriptions": 117,
    "descriptionCoverage": 100
  },
  "message": "? Database has 117 courses with 117 descriptions (100% coverage)"
}
```

---

## ?? Beklenen Sonuçlar

### Database Ýçeriði

| Tablo | Beklenen Satýr | Açýklama |
|-------|----------------|----------|
| AspNetUsers | 7 | 1 admin, 3 advisor, 3 student |
| AspNetRoles | 3 | Admin, Advisor, Student |
| Courses | 117 | Tüm müfredat dersleri |
| CourseCategories | 13 | Ders kategorileri |
| Prerequisites | 13 | Ön koþul dersleri |

### Default Kullanýcýlar

| Email | Þifre | Rol |
|-------|-------|-----|
| admin@local | Admin123! | Admin |
| advisor1@local | Advisor123! | Advisor |
| advisor2@local | Advisor123! | Advisor |
| advisor3@local | Advisor123! | Advisor |
| student1@local | Student123! | Student |
| student2@local | Student123! | Student |
| student3@local | Student123! | Student |

---

## ?? Sorun Giderme

### Sorun 1: "Cannot drop database because it is currently in use"

**Çözüm:**

```powershell
# Tüm baðlantýlarý kapat
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "
ALTER DATABASE AdvisorySystemDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE AdvisorySystemDB;
"

# Sonra normal devam et
dotnet ef database update
```

---

### Sorun 2: Migration Hatasý

**Hata:**
```
No migrations were applied. The database is already up to date.
```

**Çözüm:**

```powershell
# Migration'larý kontrol et
dotnet ef migrations list

# Database'i kesinlikle sil
dotnet ef database drop --force

# Tekrar dene
dotnet ef database update
```

---

### Sorun 3: Seeding Çalýþmýyor

**Kontrol 1:** Program.cs'de seeding var mý?

```csharp
// Program.cs
try
{
    await IdentitySeeder.SeedAsync(app.Services);
    await CourseSeeder.SeedCoursesAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error while seeding data");
}
```

**Kontrol 2:** CourseSeeder mevcut dersleri kontrol ediyor

```csharp
// CourseSeeder.cs
public static async Task SeedCoursesAsync(IServiceProvider sp)
{
    using var scope = sp.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

  if (await db.Courses.AnyAsync())
    {
 return; // ?? Mevcut dersler varsa ekleme yapmaz!
    }
    
    // ... seeding kodu
}
```

**Çözüm:** Database'i tamamen sil, yoksa seeding çalýþmaz.

---

### Sorun 4: Türkçe Karakterler Bozuk

**Kontrol:**

```sql
SELECT CourseName FROM Courses WHERE CourseCode = 'BÝL332';
-- Beklenen: ÝÞLETÝM SÝSTEMLERÝ
-- Yanlýþ: ??LET?M S?STEMLER?
```

**Çözüm:**

```sql
-- Database collation'ý kontrol et
SELECT DATABASEPROPERTYEX('AdvisorySystemDB', 'Collation');

-- Turkish_CI_AS olmalý
-- Deðilse database'i yeniden oluþtur:
ALTER DATABASE AdvisorySystemDB COLLATE Turkish_CI_AS;
```

Sonra dersleri yeniden seed et.

---

## ?? Script Dosyalarý

| Dosya | Açýklama | Kullaným |
|-------|----------|----------|
| `rebuild-database.ps1` | ? Tam otomatik | `.\rebuild-database.ps1` |
| `quick-rebuild.ps1` | Hýzlý manuel | `.\quick-rebuild.ps1` |
| `check-descriptions.sql` | SQL kontrol | SQL Editor'de çalýþtýr |

---

## ? Baþarý Checklist

Database rebuild baþarýlýysa:

- [x] `dotnet ef database drop` ? Baþarýlý
- [x] `dotnet ef database update` ? 6 migration uygulandý
- [x] `dotnet run` ? Seeding çalýþtý
- [x] SQL: `SELECT COUNT(*) FROM Courses` ? **117**
- [x] SQL: `SELECT COUNT(Description) FROM Courses` ? **117**
- [x] API: `/courses/diagnostics` ? `descriptionCoverage: 100`
- [x] Türkçe karakterler doðru ? Ý, Þ, Ð, Ü, Ö, Ç

---

## ?? Ýþlem Sýrasý (Özet)

```powershell
# 1. Otomatik (Önerilen)
.\rebuild-database.ps1
# EVET yaz ? Bekle ? Bitti ?

# 2. Manuel
dotnet ef database drop --force
dotnet ef database update
dotnet run
# 20 saniye bekle
Ctrl+C

# 3. Kontrol
# SQL'de: SELECT COUNT(*), COUNT(Description) FROM Courses;
# Beklenen: 117, 117
```

---

## ?? Sorun mu Var?

Þu bilgileri paylaþýn:

1. **Hangi script kullandýnýz?**
2. **Hata mesajý nedir?**
3. **SQL kontrol sonucu:**
   ```sql
   SELECT COUNT(*) as Total, 
          COUNT(Description) as WithDesc 
   FROM Courses;
   ```

---

**Son Güncelleme:** 2025-01-07  
**Durum:** ? Tüm araçlar hazýr
