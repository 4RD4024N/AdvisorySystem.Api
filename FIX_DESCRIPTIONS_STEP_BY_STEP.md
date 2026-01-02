# ?? Ders Açýklamalarýný Ekleme - Adým Adým Rehber

**Durum:** Database'de `Courses` tablosunda `Description` field'larý boþ  
**Hedef:** Tüm 117 dersi açýklamalarýyla birlikte yüklemek

---

## ? HIZLI ÇÖZÜM (5 Dakika)

### Seçenek 1: Otomatik Script (Önerilen)

```powershell
# PowerShell'i yönetici olarak aç
cd C:\Users\arda0\source\repos\AdvisorySystem.Api

# Scripti çalýþtýr
.\reset-courses.ps1
```

**Ne yapar:**
1. ? Mevcut dersleri siler
2. ? Uygulamayý baþlatýr (15 saniye)
3. ? Seeding otomatik çalýþýr
4. ? Sonuçlarý gösterir

---

### Seçenek 2: Manuel (Adým Adým)

#### Adým 1: Dersleri Sil

**Visual Studio'da:**
1. View ? SQL Server Object Explorer
2. (localdb)\MSSQLLocalDB ? Databases ? AdvisorySystemDB
3. Sað týk ? New Query
4. Þu SQL'i yapýþtýr ve çalýþtýr:

```sql
DELETE FROM StudentCourseSections;
DELETE FROM StudentCourses;
DELETE FROM CourseSchedules;
DELETE FROM Prerequisites;
DELETE FROM Courses;
DELETE FROM CourseCategories;
```

**VEYA PowerShell'de:**

```powershell
.\quick-reset.ps1
```

---

#### Adým 2: Uygulamayý Baþlat

```powershell
dotnet run
```

**Konsol çýktýsýný izleyin:**
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
    Executed DbCommand (xms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      INSERT INTO [CourseCategories] ...
      INSERT INTO [Courses] ...
```

**Seeding tamamlandýðýnda göreceksiniz:**
```
? 13 categories seeded
? 117 courses seeded
? 13 prerequisites seeded
```

**Ardýndan:**
- Ctrl+C ile uygulamayý durdurun
- Veya çalýþýr halde býrakýn

---

#### Adým 3: Kontrol Et

**SQL ile Kontrol:**

Visual Studio SQL Server Object Explorer'da:

```sql
-- Hýzlý kontrol
SELECT COUNT(*) as ToplamDers,
       COUNT(Description) as Aciklamali
FROM Courses;

-- Detaylý kontrol
SELECT TOP 5 
    CourseCode,
    CourseName,
    LEFT(Description, 50) + '...' as Description
FROM Courses
ORDER BY CourseCode;
```

**Beklenen Sonuç:**
```
ToplamDers  Aciklamali
117       117
```

**VEYA SQL Scripti Kullan:**

```powershell
# Visual Studio SQL Editor'de aç
check-descriptions.sql
```

F5 ile çalýþtýr ? Detaylý rapor göreceksiniz

---

## ?? Kontrol Checklist

| Kontrol | Komut | Beklenen |
|---------|-------|----------|
| ? Toplam Ders | `SELECT COUNT(*) FROM Courses` | 117 |
| ? Açýklamalý | `SELECT COUNT(*) FROM Courses WHERE Description IS NOT NULL` | 117 |
| ? Kategoriler | `SELECT COUNT(*) FROM CourseCategories` | 13 |
| ? Türkçe Karakter | `SELECT CourseName FROM Courses WHERE CourseCode = 'BÝL332'` | ÝÞLETÝM SÝSTEMLERÝ |

---

## ?? Test Örnekleri

### Test 1: SQL'de Direkt Kontrol

```sql
-- BÝL101 dersini kontrol et
SELECT 
    CourseCode,
    CourseName,
 Description,
    LEN(Description) as DescLength
FROM Courses
WHERE CourseCode = 'BÝL101';
```

**Beklenen:**
```
CourseCode: BÝL101
CourseName: BÝLGÝSAYAR YAZILIMI I
Description: Programlama temellerini ve yazýlým geliþtirme süreçlerinin ilk adýmlarýný öðretir.
DescLength: 82
```

---

### Test 2: API Diagnostics

```bash
# Uygulamayý baþlat
dotnet run

# Baþka bir terminalde
curl https://localhost:44375/api/courses/diagnostics
```

**Beklenen:**
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

### Test 3: Frontend'de Kontrol

```javascript
// Browser console'da
fetch('https://localhost:44375/api/courses/1', {
  headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
})
.then(r => r.json())
.then(data => {
  console.log('? Course:', data.courseName);
  console.log('? Description:', data.description);
  console.log('? Length:', data.description?.length);
});
```

**Beklenen Output:**
```
? Course: BÝLGÝSAYAR YAZILIMI I
? Description: Programlama temellerini ve yazýlým geliþtirme süreçlerinin ilk adýmlarýný öðretir.
? Length: 82
```

---

## ?? Sorun Giderme

### Sorun 1: "sqlcmd is not recognized"

**Neden:** SQL Server Command Line Utilities yüklü deðil

**Çözüm:**

**A) Visual Studio kullan:**
1. View ? SQL Server Object Explorer
2. Manuel SQL çalýþtýr (yukarýda gösterildi)

**B) SQLCMD yükle:**
```powershell
# Microsoft SQL Server Command Line Utilities indir
# https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility
```

---

### Sorun 2: Seeding çalýþmýyor

**Kontrol 1:** CourseSeeder çaðrýlýyor mu?

`Program.cs` dosyasýnda:
```csharp
await CourseSeeder.SeedCoursesAsync(app.Services);
```

**Kontrol 2:** Mevcut dersler var mý?

```csharp
// CourseSeeder.cs içinde
if (await db.Courses.AnyAsync())
{
    return; // ?? Mevcut dersler varsa ekleme yapmaz!
}
```

**Çözüm:** Dersleri tamamen sil (Adým 1'i tekrarla)

---

### Sorun 3: Açýklamalar hala null

**Kontrol 1:** Doðru CourseSeeder kullanýlýyor mu?

```csharp
// CourseSeeder.cs - Açýklama örneði
new() { 
    CourseCode = "BÝL101", 
    CourseName = "BÝLGÝSAYAR YAZILIMI I",
    Description = "Programlama temellerini...", // ? Bu satýr var mý?
    // ...
}
```

**Kontrol 2:** En son kodu kullanýyor musunuz?

```bash
git pull origin master
```

**Çözüm:** CourseSeeder.cs dosyasýný kontrol edin, açýklamalar ekli mi?

---

## ?? Hazýr Araçlar

| Dosya | Açýklama | Kullaným |
|-------|----------|----------|
| `reset-courses.ps1` | Otomatik tam çözüm | `.\reset-courses.ps1` |
| `quick-reset.ps1` | Sadece sil | `.\quick-reset.ps1` |
| `check-descriptions.sql` | Detaylý rapor | SQL Editor'de çalýþtýr |

---

## ? Baþarý Kriterleri

Tüm bunlar doðruysa iþlem baþarýlý:

- [x] `SELECT COUNT(*) FROM Courses` ? **117**
- [x] `SELECT COUNT(*) FROM Courses WHERE Description IS NOT NULL` ? **117**
- [x] API diagnostics ? `"descriptionCoverage": 100`
- [x] Frontend'de açýklamalar görünüyor
- [x] Türkçe karakterler doðru (Ý, Þ, Ð, Ü, Ö, Ç)

---

## ?? Son Adým

Database güncellenince frontend'i yenileyin:

```bash
# Frontend'i yeniden baþlat
npm run dev
```

Tarayýcýyý yenile (Ctrl+F5) ve dersler sayfasýna git.

**Artýk tüm derslerin açýklamalarýný görmelisiniz!** ??

---

## ?? Hala Sorun mu Var?

Þu bilgileri paylaþýn:

1. **SQL Sonucu:**
```sql
SELECT COUNT(*), COUNT(Description) FROM Courses;
```

2. **API Diagnostics:**
```
https://localhost:44375/api/courses/diagnostics
```

3. **Sample Course:**
```sql
SELECT TOP 1 * FROM Courses;
```

---

**Son Güncelleme:** 2025-01-07  
**Durum:** ? Tüm araçlar hazýr
