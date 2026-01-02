# ?? Öðrenci Ders Programý Sorunu - Çözüldü!

**Tarih:** 2025-01-07  
**Sorun:** Öðrenciler ders seçince ders programýnda görünmüyor  
**Çözüm:** StudentCourseSection kaydý ve yeni my-schedule endpoint'i eklendi

---

## ?? Sorunun Nedeni

Öðrenciler ders seçerken **2 farklý tabloya** kayýt yapýlmasý gerekiyor ama sadece 1 tanesi yapýlýyordu:

| Tablo | Ne Ýçin | Durum |
|-------|---------|-------|
| `StudentCourses` | Ders kayýtlarý, notlar, tamamlama durumu | ? Çalýþýyordu |
| `StudentCourseSections` | Ders programý, zaman çizelgesi, section bilgisi | ? EKSIKT!|

**Sonuç:** Öðrenci dersi seçiyor ama ders programýnda göremiyordu çünkü `StudentCourseSections` boþtu.

---

## ? YAPILAN DEÐÝÞÝKLÝKLER

### 1. Enroll Endpoint Güncellendi

#### Eskisi:
```csharp
[HttpPost("enroll")]
public async Task<IActionResult> EnrollCourse([FromBody] EnrollCourseDto dto)
{
    var studentCourse = new StudentCourse { ... };
    _db.StudentCourses.Add(studentCourse);
    await _db.SaveChangesAsync();
    // ? StudentCourseSection eklenmiyordu!
}
```

#### Yenisi:
```csharp
[HttpPost("enroll")]
public async Task<IActionResult> EnrollCourse([FromBody] EnrollCourseDto dto)
{
    // 1. Schedule kontrolü ?
    var scheduleExists = await _db.CourseSchedules
    .AnyAsync(cs => cs.CourseId == dto.CourseId && cs.Semester == dto.Semester);

    if (!scheduleExists)
   return BadRequest(new { error = "Course schedule not found" });

    // 2. StudentCourse kaydý ?
    var studentCourse = new StudentCourse { ... };
    _db.StudentCourses.Add(studentCourse);

    // 3. StudentCourseSection kaydý ? YENÝ!
    var defaultSection = await _db.CourseSchedules
        .Where(cs => cs.CourseId == dto.CourseId && cs.Semester == dto.Semester)
        .OrderBy(cs => cs.SectionCode)
        .FirstOrDefaultAsync();

    if (defaultSection != null)
    {
        var section = new StudentCourseSection
        {
     StudentId = userId,
            CourseId = dto.CourseId,
            SectionCode = defaultSection.SectionCode,
      Semester = dto.Semester ?? 0
        };
        _db.StudentCourseSections.Add(section);
    }

    await _db.SaveChangesAsync();
}
```

### 2. Yeni Endpoint Eklendi: `/api/student-courses/my-schedule`

```csharp
[HttpGet("my-schedule")]
public async Task<IActionResult> GetMySchedule([FromQuery] int? semester = null)
{
    // Öðrencinin kayýtlý olduðu section'larý al
    var enrolledSections = await _db.StudentCourseSections
        .Where(scs => scs.StudentId == userId)
   .Include(scs => scs.Course)
        .ThenInclude(c => c.Category)
   .ToListAsync();

    // Her section için schedule bilgilerini al
    // Haftalýk programa dönüþtür
  
    return Ok(new { totalCourses, semester, courses, weeklySchedule });
}
```

---

## ?? API KULLANIMI

### 1. Derse Kayýt Ol

```http
POST /api/student-courses/enroll
Authorization: Bearer {student-token}
Content-Type: application/json

{
  "courseId": 1,
  "semester": 1
}
```

**Response (Baþarýlý):**
```json
{
  "message": "Enrolled successfully",
  "enrollmentId": 5,
  "sectionCode": "A",
  "semester": 1
}
```

**Response (Hata - Schedule Yok):**
```json
{
"error": "Course schedule not found",
  "message": "This course doesn't have a schedule for semester 1. Please contact admin to generate schedules."
}
```

---

### 2. Ders Programýmý Getir

```http
GET /api/student-courses/my-schedule?semester=1
Authorization: Bearer {student-token}
```

**Query Parameters:**
- `semester` (optional): Belirli bir yarýyýl için filtreleme

**Response:**
```json
{
  "totalCourses": 5,
  "semester": "1",
  "courses": [
    {
      "courseId": 1,
      "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
      "credits": 3,
      "ects": 5,
   "category": "Birinci Yarýyýl (Güz)",
      "sectionCode": "A",
      "semester": 1,
      "isCompleted": false,
    "grade": null,
      "sessions": [
   {
          "dayOfWeek": "Monday",
          "dayOfWeekNumber": 1,
      "startTime": "09:00",
       "endTime": "10:50",
          "roomNumber": "A101",
  "instructorName": "Prof. Dr. Ali Veli",
          "isTheory": true,
      "sessionNumber": 1
        },
   {
    "dayOfWeek": "Wednesday",
          "dayOfWeekNumber": 3,
  "startTime": "13:00",
        "endTime": "14:50",
          "roomNumber": "LAB1",
          "instructorName": "Arþ. Gör. Ayþe Yýlmaz",
          "isTheory": false,
    "sessionNumber": 2
        }
      ]
    }
  ],
  "weeklySchedule": {
    "Monday": [
   {
        "courseCode": "BÝL101",
        "courseName": "BÝLGÝSAYAR YAZILIMI I",
        "sectionCode": "A",
    "startTime": "09:00",
    "endTime": "10:50",
        "roomNumber": "A101",
      "instructorName": "Prof. Dr. Ali Veli",
  "isTheory": true
      }
    ],
    "Tuesday": [],
    "Wednesday": [
      {
        "courseCode": "BÝL101",
        "courseName": "BÝLGÝSAYAR YAZILIMI I",
     "sectionCode": "A",
        "startTime": "13:00",
     "endTime": "14:50",
        "roomNumber": "LAB1",
        "instructorName": "Arþ. Gör. Ayþe Yýlmaz",
        "isTheory": false
      }
    ],
    "Thursday": [],
    "Friday": []
}
}
```

---

## ?? TEST SENARYOSU

### Senaryo: Öðrenci Ders Seçimi ve Program Görüntüleme

```javascript
// 1. Login
const loginRes = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
 email: 'student1@local',
    password: 'Student123!'
  })
});
const { token } = await loginRes.json();

// 2. Derse kayýt ol
const enrollRes = await fetch('/api/student-courses/enroll', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    courseId: 1,
    semester: 1
  })
});

console.log(await enrollRes.json());
// Output: { message: "Enrolled successfully", enrollmentId: 5, sectionCode: "A" }

// 3. Ders programýný getir
const scheduleRes = await fetch('/api/student-courses/my-schedule?semester=1', {
  headers: { 'Authorization': `Bearer ${token}` }
});

const schedule = await scheduleRes.json();
console.log(`Toplam ${schedule.totalCourses} ders kayýtlý`);
console.log('Pazartesi günü:', schedule.weeklySchedule.Monday);
```

---

## ?? FRONTEND ENTEGRASYONU

### React Örneði

```jsx
import { useState, useEffect } from 'react';
import api from '../services/api';

const MySchedule = () => {
  const [schedule, setSchedule] = useState(null);
  const [semester, setSemester] = useState(1);

  useEffect(() => {
    const fetchSchedule = async () => {
      try {
     const res = await api.get(`/student-courses/my-schedule?semester=${semester}`);
        setSchedule(res.data);
      } catch (error) {
        console.error('Schedule fetch error:', error);
    }
    };
    fetchSchedule();
  }, [semester]);

  if (!schedule) return <div>Loading...</div>;

  return (
<div className="schedule-container">
      <h2>Ders Programým - Yarýyýl {semester}</h2>
      
 <div className="semester-select">
        <button onClick={() => setSemester(1)}>Yarýyýl 1</button>
        <button onClick={() => setSemester(2)}>Yarýyýl 2</button>
      </div>

  <div className="weekly-schedule">
        {Object.entries(schedule.weeklySchedule).map(([day, courses]) => (
        <div key={day} className="day-column">
   <h3>{day}</h3>
            {courses.length === 0 ? (
 <p>Ders yok</p>
            ) : (
    courses.map((course, idx) => (
              <div key={idx} className="course-card">
       <div className="course-code">{course.courseCode}</div>
           <div className="course-name">{course.courseName}</div>
   <div className="time">
     {course.startTime} - {course.endTime}
         </div>
         <div className="room">{course.roomNumber}</div>
  <div className="instructor">{course.instructorName}</div>
       <div className="type">
      {course.isTheory ? '?? Teori' : '?? Uygulama'}
         </div>
      </div>
              ))
    )}
          </div>
    ))}
      </div>
    </div>
  );
};

export default MySchedule;
```

---

## ?? ÖNEMLÝ NOTLAR

### 1. Schedule Oluþturma (Admin)

Öðrencilerin derse kaydolabilmesi için **önce schedule oluþturulmalý**:

```http
POST /api/schedule/generate/{semester}
Authorization: Bearer {admin-token}
```

Admin bu endpoint'i kullanarak yarýyýl için schedule oluþturmalý.

### 2. Ders Kayýt Akýþý

```
1. Admin ? Schedule oluþturur (generate/{semester})
2. Öðrenci ? Dersleri görüntüler (GET /api/schedule/available/{semester})
3. Öðrenci ? Derse kayýt olur (POST /api/student-courses/enroll)
   ? StudentCourses tablosuna eklenir
   ? StudentCourseSections tablosuna eklenir
4. Öðrenci ? Ders programýný görür (GET /api/student-courses/my-schedule)
```

### 3. Section Seçimi

Þu an **otomatik** olarak ilk (A) section'a kaydediliyor. Ýleride section seçimi eklenebilir:

```csharp
// Gelecek geliþtirme
public record EnrollCourseDto(
    int CourseId,
  int? Semester,
 string? PreferredSection // ? Yeni parametre
);
```

---

## ?? SORUN GÝDERME

### Sorun 1: "Course schedule not found" Hatasý

**Sebep:** O ders için schedule oluþturulmamýþ.

**Çözüm:**
```http
POST /api/schedule/generate/{semester}
Authorization: Bearer {admin-token}
```

---

### Sorun 2: Ders Programý Boþ Geliyor

**Kontrol 1:** StudentCourseSections tablosunda kayýt var mý?

```sql
SELECT * FROM StudentCourseSections 
WHERE StudentId = 'user-id-123';
```

**Beklenen:** En az 1 satýr

**Kontrol 2:** CourseSchedules tablosunda o ders için schedule var mý?

```sql
SELECT cs.* 
FROM CourseSchedules cs
INNER JOIN StudentCourseSections scs ON cs.CourseId = scs.CourseId 
    AND cs.SectionCode = scs.SectionCode
WHERE scs.StudentId = 'user-id-123';
```

**Beklenen:** Her kayýtlý ders için schedule satýrlarý

---

### Sorun 3: Eski Kayýtlar Schedule'da Görünmüyor

**Sebep:** Eski kayýtlarda `StudentCourseSections` kaydý yok.

**Çözüm:** Manuel migration scripti:

```sql
-- Mevcut StudentCourses kayýtlarýný StudentCourseSections'a aktar
INSERT INTO StudentCourseSections (StudentId, CourseId, SectionCode, Semester, EnrolledAt, IsCompleted)
SELECT 
    sc.StudentId,
    sc.CourseId,
    'A' as SectionCode, -- Default section
    sc.Semester,
  sc.EnrolledAt,
 sc.IsCompleted
FROM StudentCourses sc
WHERE NOT EXISTS (
    SELECT 1 FROM StudentCourseSections scs 
    WHERE scs.StudentId = sc.StudentId 
      AND scs.CourseId = sc.CourseId
);
```

---

## ? ÖZET

| Özellik | Durum |
|---------|-------|
| Derse kayýt | ? Çalýþýyor |
| StudentCourses kaydý | ? Yapýlýyor |
| StudentCourseSections kaydý | ? YENÝ - Eklendi |
| Schedule kontrolü | ? YENÝ - Eklendi |
| Ders programý görüntüleme | ? YENÝ - Endpoint eklendi |
| Haftalýk program | ? YENÝ - weeklySchedule response |
| Section bilgisi | ? YENÝ - sectionCode döndürülüyor |

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-07  
**Durum:** ? Tamamlandý ve Test Edilmeye Hazýr

---

## ?? SON ADIMLAR

1. **Admin:** Schedule oluþtur
```
POST /api/schedule/generate/1
```

2. **Öðrenci:** Derse kayýt ol
```
POST /api/student-courses/enroll
{ "courseId": 1, "semester": 1 }
```

3. **Öðrenci:** Programýný gör
```
GET /api/student-courses/my-schedule?semester=1
```

4. **Frontend:** Haftalýk takvim görünümü ile göster

?? **Artýk öðrenciler ders programlarýný görebilir!**
