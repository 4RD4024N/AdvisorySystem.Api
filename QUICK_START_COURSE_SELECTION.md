# ?? Ders Seçimi Sistemi - Hýzlý Baþlangýç

## ?? Ýstenen Özellikler

1. ? **Courses sekmesine dokunma** - Sadece ders listesi
2. ? **Ders Programý sekmesi** - Buradan ders seçimi
3. ? **Backend GÜN-SAAT bilgisi gönderir**
4. ? **Öðrenci ders ekleyebilir/çýkarabilir**

---

## ?? YENÝ ENDPOINT'LER

### 1. Ders Seçimi Listesi
```
GET /api/course-selection/available?semester=1
```

**Response önemli alanlar:**
```json
{
  "isEnrolled": false,  // ? Öðrenci bu derse kayýtlý mý?
  "schedule": [         // ? GÜN-SAAT BÝLGÝSÝ
    {
"dayOfWeek": "Monday",
      "startTime": "09:00",
  "endTime": "10:50",
      "roomNumber": "A101",
    "sessionType": "Teori"
}
  ]
}
```

### 2. Derse Kayýt
```
POST /api/course-selection/enroll
Body: { courseId, sectionCode, semester }
```

**Özellikler:**
- ? Zaman çakýþmasý kontrolü
- ? Kapasite kontrolü
- ? Otomatik ders programýna ekleme

### 3. Dersten Çýk
```
DELETE /api/course-selection/unenroll
Body: { courseId, sectionCode, semester }
```

### 4. Ders Programým
```
GET /api/student-courses/my-schedule?semester=1
```

**Response:**
```json
{
  "weeklySchedule": {
    "Pazartesi": [
      {
        "courseCode": "BÝL101",
        "startTime": "09:00",
        "endTime": "10:50",
        "roomNumber": "A101"
      }
    ],
"Salý": [...],
    ...
  }
}
```

---

## ?? FRONTEND KOD

### Ders Seçimi Ekraný

```jsx
const CourseSelection = () => {
  const [courses, setCourses] = useState([]);

  useEffect(() => {
    api.get('/course-selection/available?semester=1')
    .then(res => setCourses(res.data.courses));
  }, []);

  const handleEnroll = async (course) => {
    try {
      await api.post('/course-selection/enroll', {
        courseId: course.courseId,
        sectionCode: course.sectionCode,
        semester: course.semester
  });
      alert('? Derse kayýt oldunuz!');
  } catch (error) {
      alert(`? ${error.response.data.error}`);
    }
  };

  return (
    <div>
      {courses.map(course => (
    <div key={course.courseId}>
          <h3>{course.courseCode} - {course.courseName}</h3>
          
        {/* ? DERS SAATLERÝ */}
          <div className="schedule">
  {course.schedule.map((s, i) => (
    <div key={i}>
     ?? {s.dayOfWeek} {s.startTime}-{s.endTime} 
 ?? {s.roomNumber}
          </div>
            ))}
       </div>

          {/* KAYIT BUTONU */}
   {course.isEnrolled ? (
   <button disabled>? Kayýtlý</button>
          ) : (
      <button onClick={() => handleEnroll(course)}>
   Derse Kayýt Ol
            </button>
    )}
        </div>
      ))}
    </div>
  );
};
```

---

## ?? ÖN KOÞUL

Admin önce schedule oluþturmalý:
```
POST /api/schedule/generate/1
```

Yoksa öðrenci kayýt olamaz!

---

## ? ÖZETÝ

| Özellik | Endpoint | Ne Yapýyor |
|---------|----------|------------|
| Ders listesi (schedule'lý) | GET /course-selection/available | Tüm dersleri GÜN-SAAT ile gösterir |
| Derse kayýt | POST /course-selection/enroll | Öðrenciyi derse ekler |
| Dersten çýk | DELETE /course-selection/unenroll | Öðrenciyi dersten çýkarýr |
| Programým | GET /student-courses/my-schedule | Haftalýk programýný gösterir |

---

**Durum:** ? Hazýr  
**Test:** `COURSE_SELECTION_SYSTEM.md` dokümanýna bakýn  
**Frontend:** Schedule bilgisi ile ders ekleme/çýkarma yapabilir! ??
