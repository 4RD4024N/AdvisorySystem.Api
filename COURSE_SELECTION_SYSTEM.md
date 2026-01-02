# ?? Ders Seçimi ve Ders Programý Sistemi - Tam Rehber

**Tarih:** 2025-01-07  
**Durum:** ? Tamamlandý ve Hazýr  
**Backend:** .NET 8 API

---

## ?? SÝSTEM AKIÞI

### 1. Courses Sekmesi
- **Amaç:** Sadece ders listesini gösterir
- **Deðiþiklik:** YOK
- **Endpoint:** `/api/courses`

### 2. Ders Programý Sekmesi (YENÝ)
- **Amaç:** Öðrenciler buradan ders seçip programlarýna ekler
- **Özellikler:**
  - ? Her dersin **gün ve saat** bilgisi gösterilir
  - ? Öðrenci hangi derslere kayýtlý **iþaretli** gelir
  - ? Ders ekleme/çýkarma yapýlabilir
  - ? Zaman çakýþmasý kontrolü yapýlýr
  - ? Kapasite kontrolü yapýlýr

---

## ?? YENÝ API ENDPOINT'LERÝ

### 1. Ders Seçimi Ýçin Dersler `/api/course-selection/available`

#### Request:
```http
GET /api/course-selection/available?semester=1
Authorization: Bearer {student-token}
```

#### Query Parameters:
| Parametre | Tip | Zorunlu | Açýklama |
|-----------|-----|---------|----------|
| `semester` | int | Hayýr | Yarýyýl filtresi (boþsa tümü) |

#### Response:
```json
{
  "totalCourses": 25,
  "semester": 1,
  "courses": [
    {
      // Ders Bilgileri
  "courseId": 3,
  "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
      "description": "Programlama temellerini ve yazýlým geliþtirme süreçlerinin ilk adýmlarýný öðretir.",
      "credits": 3,
      "ects": 5,
      "theoryHours": 3,
      "practiceHours": 1,
      "isElective": false,
      "category": {
        "id": 2,
        "name": "Birinci Yarýyýl (Güz)"
      },
      
// Section Bilgileri
  "sectionCode": "A",
      "semester": 1,
   "instructor": "Prof. Dr. Ali Veli",
      "maxCapacity": 50,
      "enrolledCount": 23,
      "availableSeats": 27,
      "isFull": false,
      
      // Öðrenci Durumu
      "isEnrolled": false, // ? Öðrenci bu derse kayýtlý mý?
      
    // ? Schedule Bilgileri (GÜN-SAAT)
      "schedule": [
        {
          "dayOfWeek": "Monday",
          "dayOfWeekNumber": 1,
     "startTime": "09:00",
   "endTime": "10:50",
       "roomNumber": "A101",
          "isTheory": true,
       "sessionType": "Teori",
      "sessionNumber": 1,
       "timeSlot": "Monday 09:00-10:50"
        },
        {
     "dayOfWeek": "Wednesday",
       "dayOfWeekNumber": 3,
     "startTime": "13:00",
   "endTime": "14:50",
     "roomNumber": "LAB1",
"isTheory": false,
 "sessionType": "Uygulama",
    "sessionNumber": 2,
    "timeSlot": "Wednesday 13:00-14:50"
        }
      ]
  }
  ]
}
```

---

### 2. Derse Kayýt Ol `/api/course-selection/enroll`

#### Request:
```http
POST /api/course-selection/enroll
Authorization: Bearer {student-token}
Content-Type: application/json

{
  "courseId": 3,
  "sectionCode": "A",
  "semester": 1
}
```

#### Response (Baþarýlý):
```json
{
  "message": "Enrolled successfully",
"enrollmentId": 15,
  "courseCode": "BÝL101",
  "courseName": "BÝLGÝSAYAR YAZILIMI I",
  "sectionCode": "A",
  "semester": 1,
  "schedule": [
    {
      "dayOfWeek": "Monday",
      "startTime": "09:00",
   "endTime": "10:50",
"roomNumber": "A101",
      "isTheory": true
    },
    {
      "dayOfWeek": "Wednesday",
      "startTime": "13:00",
      "endTime": "14:50",
  "roomNumber": "LAB1",
      "isTheory": false
    }
  ]
}
```

#### Response (Hata - Zaman Çakýþmasý):
```json
{
  "error": "Schedule conflict",
"message": "Schedule conflict on Monday with BÝL105",
  "conflictingCourse": {
    "courseCode": "BÝL105",
    "courseName": "PROGRAMLAMA LABORATUVARI I",
    "day": "Monday",
    "time": "09:00-10:50"
  }
}
```

#### Response (Hata - Kapasite Dolu):
```json
{
  "error": "Course section is full"
}
```

#### Response (Hata - Zaten Kayýtlý):
```json
{
"error": "Already enrolled in this course"
}
```

---

### 3. Dersten Çýk `/api/course-selection/unenroll`

#### Request:
```http
DELETE /api/course-selection/unenroll
Authorization: Bearer {student-token}
Content-Type: application/json

{
  "courseId": 3,
  "sectionCode": "A",
  "semester": 1
}
```

#### Response:
```json
{
  "message": "Unenrolled successfully"
}
```

---

### 4. Ders Programýmý Görüntüle `/api/student-courses/my-schedule`

#### Request:
```http
GET /api/student-courses/my-schedule?semester=1
Authorization: Bearer {student-token}
```

#### Response:
```json
{
  "totalCourses": 5,
  "completedCourses": 0,
  "totalCredits": 15,
  "totalECTS": 25,
  "semester": "1",
  "courses": [
    {
      "courseId": 3,
  "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
      "description": "Programlama temellerini...",
      "credits": 3,
      "ects": 5,
      "category": "Birinci Yarýyýl (Güz)",
      "sectionCode": "A",
  "semester": 1,
      "isCompleted": false,
  "grade": null,
      "sessions": [
        {
      "scheduleId": 10,
          "dayOfWeek": "Monday",
          "dayOfWeekNumber": 1,
  "dayName": "Pazartesi",
    "startTime": "09:00",
          "endTime": "10:50",
       "timeSlot": "09:00-10:50",
    "roomNumber": "A101",
          "instructorName": "Prof. Dr. Ali Veli",
     "isTheory": true,
          "sessionType": "Teori",
          "sessionNumber": 1,
    "durationMinutes": 110
        }
      ]
    }
],
  "weeklySchedule": {
    "Pazartesi": [
      {
        "courseId": 3,
    "courseCode": "BÝL101",
     "courseName": "BÝLGÝSAYAR YAZILIMI I",
        "sectionCode": "A",
    "startTime": "09:00",
  "endTime": "10:50",
        "timeSlot": "09:00-10:50",
        "roomNumber": "A101",
        "instructorName": "Prof. Dr. Ali Veli",
        "isTheory": true,
   "sessionType": "Teori",
        "durationMinutes": 110
      }
    ],
    "Salý": [],
    "Çarþamba": [
      {
        "courseId": 3,
        "courseCode": "BÝL101",
        "courseName": "BÝLGÝSAYAR YAZILIMI I",
        "sectionCode": "A",
"startTime": "13:00",
   "endTime": "14:50",
   "timeSlot": "13:00-14:50",
        "roomNumber": "LAB1",
        "instructorName": "Arþ. Gör. Ayþe Yýlmaz",
        "isTheory": false,
        "sessionType": "Uygulama",
        "durationMinutes": 110
      }
    ],
    "Perþembe": [],
    "Cuma": []
  }
}
```

---

## ?? FRONTEND ENTEGRASYONU

### React Component Örneði

```jsx
import { useState, useEffect } from 'react';
import api from '../services/api';

const CourseSelection = () => {
  const [availableCourses, setAvailableCourses] = useState([]);
  const [semester, setSemester] = useState(1);
  const [loading, setLoading] = useState(false);

  // Mevcut dersleri yükle
  useEffect(() => {
    fetchAvailableCourses();
  }, [semester]);

  const fetchAvailableCourses = async () => {
    setLoading(true);
    try {
      const res = await api.get(`/course-selection/available?semester=${semester}`);
 setAvailableCourses(res.data.courses);
    } catch (error) {
      console.error('Failed to fetch courses:', error);
    } finally {
      setLoading(false);
    }
  };

  // Derse kayýt ol
  const handleEnroll = async (course) => {
    try {
      const res = await api.post('/course-selection/enroll', {
 courseId: course.courseId,
     sectionCode: course.sectionCode,
        semester: course.semester
      });
      
      alert(`? ${res.data.message}\n${res.data.courseCode} - ${res.data.courseName}`);
      fetchAvailableCourses(); // Listeyi yenile
 } catch (error) {
      if (error.response?.data?.conflictingCourse) {
        const conflict = error.response.data.conflictingCourse;
        alert(`? Zaman Çakýþmasý!\n${conflict.courseCode} dersi ile ${conflict.day} günü ${conflict.time} saatinde çakýþýyor.`);
      } else {
        alert(`? ${error.response?.data?.error || 'Kayýt baþarýsýz'}`);
 }
    }
  };

  // Dersten çýk
  const handleUnenroll = async (course) => {
    if (!confirm(`${course.courseCode} dersinden çýkmak istediðinize emin misiniz?`)) return;
    
  try {
      await api.delete('/course-selection/unenroll', {
        data: {
          courseId: course.courseId,
          sectionCode: course.sectionCode,
          semester: course.semester
        }
    });
      
      alert('? Dersten çýkýldý');
      fetchAvailableCourses();
    } catch (error) {
alert(`? ${error.response?.data?.error || 'Ýþlem baþarýsýz'}`);
    }
  };

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div className="course-selection">
<h2>Ders Seçimi - Yarýyýl {semester}</h2>
      
      <div className="semester-selector">
        {[1, 2, 3, 4, 5, 6, 7, 8].map(s => (
        <button 
            key={s}
            onClick={() => setSemester(s)}
            className={semester === s ? 'active' : ''}
          >
     Yarýyýl {s}
        </button>
        ))}
    </div>

      <div className="courses-grid">
     {availableCourses.map(course => (
  <div key={`${course.courseId}-${course.sectionCode}`} className="course-card">
          <div className="course-header">
   <h3>{course.courseCode}</h3>
  <span className="section-badge">Þube: {course.sectionCode}</span>
     {course.isEnrolled && <span className="enrolled-badge">? Kayýtlý</span>}
   </div>
            
            <h4>{course.courseName}</h4>
            <p className="description">{course.description}</p>
         
            <div className="course-info">
        <span>?? {course.credits} Kredi</span>
        <span>?? {course.ects} ECTS</span>
        <span>?? {course.enrolledCount}/{course.maxCapacity}</span>
            </div>
            
     {/* ? Ders Programý (GÜN-SAAT) */}
         <div className="schedule">
         <h5>?? Ders Saatleri:</h5>
        {course.schedule.map((session, idx) => (
  <div key={idx} className="schedule-item">
          <span className="day">{getDayName(session.dayOfWeek)}</span>
     <span className="time">{session.startTime} - {session.endTime}</span>
         <span className="room">?? {session.roomNumber}</span>
          <span className="type">{session.sessionType}</span>
                </div>
      ))}
          </div>

            <div className="instructor">
      ????? {course.instructor}
      </div>
            
            {/* Kayýt/Çýkýþ Butonu */}
            {course.isEnrolled ? (
         <button 
                className="btn-unenroll" 
        onClick={() => handleUnenroll(course)}
              >
           ? Dersten Çýk
      </button>
     ) : course.isFull ? (
   <button className="btn-full" disabled>
     ? Kapasite Dolu
              </button>
   ) : (
   <button 
       className="btn-enroll" 
 onClick={() => handleEnroll(course)}
     >
           ? Derse Kayýt Ol
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

// Gün isimlerini Türkçe'ye çevir
const getDayName = (day) => {
  const days = {
    'Monday': 'Pazartesi',
    'Tuesday': 'Salý',
    'Wednesday': 'Çarþamba',
    'Thursday': 'Perþembe',
    'Friday': 'Cuma'
  };
  return days[day] || day;
};

export default CourseSelection;
```

---

### Haftalýk Program Görünümü

```jsx
const WeeklySchedule = () => {
  const [schedule, setSchedule] = useState(null);
  const [semester, setSemester] = useState(1);

  useEffect(() => {
    fetchSchedule();
  }, [semester]);

  const fetchSchedule = async () => {
    try {
      const res = await api.get(`/student-courses/my-schedule?semester=${semester}`);
      setSchedule(res.data);
    } catch (error) {
    console.error('Failed to fetch schedule:', error);
    }
  };

  if (!schedule) return <div>Yükleniyor...</div>;

  const days = ['Pazartesi', 'Salý', 'Çarþamba', 'Perþembe', 'Cuma'];
  const timeSlots = generateTimeSlots('08:00', '18:00'); // 08:00-18:00 arasý

  return (
    <div className="weekly-schedule">
  <h2>Haftalýk Ders Programý - Yarýyýl {semester}</h2>
    
      <div className="stats">
        <div className="stat">
     <span>?? Toplam Ders:</span>
<strong>{schedule.totalCourses}</strong>
        </div>
<div className="stat">
          <span>?? Toplam Kredi:</span>
      <strong>{schedule.totalCredits}</strong>
        </div>
        <div className="stat">
    <span>?? Toplam ECTS:</span>
          <strong>{schedule.totalECTS}</strong>
     </div>
      </div>

      <table className="schedule-table">
 <thead>
     <tr>
   <th>Saat</th>
 {days.map(day => (
   <th key={day}>{day}</th>
   ))}
    </tr>
        </thead>
        <tbody>
{timeSlots.map(time => (
            <tr key={time}>
              <td className="time-cell">{time}</td>
    {days.map(day => {
      const daySchedule = schedule.weeklySchedule[day] || [];
  const courseAtTime = daySchedule.find(c => 
       isTimeBetween(time, c.startTime, c.endTime)
                );

           return (
          <td key={day} className={courseAtTime ? 'has-course' : ''}>
        {courseAtTime && (
          <div className="course-cell">
  <div className="course-code">{courseAtTime.courseCode}</div>
              <div className="course-time">{courseAtTime.timeSlot}</div>
     <div className="course-room">?? {courseAtTime.roomNumber}</div>
   <div className="course-type">{courseAtTime.sessionType}</div>
    </div>
          )}
             </td>
     );
     })}
         </tr>
  ))}
        </tbody>
      </table>
    </div>
  );
};

// Helper functions
const generateTimeSlots = (start, end) => {
  const slots = [];
  let current = start;
  
  while (current < end) {
    slots.push(current);
    current = addHours(current, 1);
  }
  
  return slots;
};

const addHours = (time, hours) => {
  const [h, m] = time.split(':').map(Number);
  const newH = (h + hours).toString().padStart(2, '0');
  return `${newH}:${m.toString().padStart(2, '0')}`;
};

const isTimeBetween = (time, start, end) => {
  return time >= start && time < end;
};
```

---

## ?? CSS ÖRNEÐÝ

```css
/* Ders Kartlarý */
.course-card {
  border: 2px solid #e0e0e0;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
  background: white;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  transition: transform 0.2s;
}

.course-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0,0,0,0.15);
}

.course-card.enrolled {
  border-color: #4caf50;
  background: #f1f8f4;
}

/* Schedule Items */
.schedule-item {
  display: flex;
  gap: 12px;
  padding: 8px 12px;
  background: #f5f5f5;
  border-radius: 6px;
  margin: 8px 0;
  align-items: center;
}

.schedule-item .day {
  min-width: 80px;
  font-weight: 600;
  color: #1976d2;
}

.schedule-item .time {
  min-width: 100px;
  font-family: monospace;
}

.schedule-item .room {
  color: #666;
}

.schedule-item .type {
  margin-left: auto;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 0.85em;
  font-weight: 500;
}

.schedule-item .type:contains("Teori") {
  background: #e3f2fd;
  color: #1976d2;
}

.schedule-item .type:contains("Uygulama") {
  background: #f3e5f5;
  color: #7b1fa2;
}

/* Buttons */
.btn-enroll {
  background: #4caf50;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  width: 100%;
  margin-top: 12px;
}

.btn-enroll:hover {
  background: #45a049;
}

.btn-unenroll {
  background: #f44336;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  width: 100%;
  margin-top: 12px;
}

.btn-full {
  background: #9e9e9e;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: not-allowed;
  width: 100%;
  margin-top: 12px;
}

/* Haftalýk Program Tablosu */
.schedule-table {
  width: 100%;
border-collapse: collapse;
  margin-top: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.schedule-table th {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 15px;
  text-align: center;
  font-weight: 600;
}

.schedule-table td {
  border: 1px solid #e0e0e0;
  padding: 8px;
  text-align: center;
  min-height: 80px;
  vertical-align: top;
}

.schedule-table td.time-cell {
background: #f5f5f5;
  font-weight: 600;
  color: #666;
}

.schedule-table td.has-course {
  background: #e3f2fd;
}

.course-cell {
  background: white;
  padding: 12px;
  border-radius: 6px;
  border-left: 4px solid #1976d2;
  text-align: left;
}

.course-cell .course-code {
  font-weight: 700;
  color: #1976d2;
  font-size: 1.1em;
  margin-bottom: 4px;
}

.course-cell .course-time {
  font-family: monospace;
  color: #666;
font-size: 0.9em;
}

.course-cell .course-room {
  color: #888;
  font-size: 0.85em;
  margin-top: 4px;
}
```

---

## ? SÝSTEM ÖZELLÝKLERÝ

| Özellik | Durum | Açýklama |
|---------|-------|----------|
| Ders listesi + zaman bilgisi | ? | Her dersin gün-saat bilgisi gösteriliyor |
| Öðrenci kayýt durumu | ? | `isEnrolled` field ile iþaretli |
| Ders ekleme | ? | POST `/course-selection/enroll` |
| Ders çýkarma | ? | DELETE `/course-selection/unenroll` |
| Zaman çakýþmasý kontrolü | ? | Otomatik kontrol |
| Kapasite kontrolü | ? | Dolu dersler engelliyor |
| Haftalýk program görünümü | ? | Türkçe günler ile |
| Section (þube) desteði | ? | A, B, C þubeleri |

---

## ?? KURULUM VE TEST

### 1. Schedule Oluþtur (Admin)

```http
POST /api/schedule/generate/1
Authorization: Bearer {admin-token}
```

Bu olmadan sistem çalýþmaz!

### 2. Ders Seçimi Ekraný Test

```javascript
// Frontend'de
const courses = await api.get('/course-selection/available?semester=1');
console.log(courses.data.courses[0].schedule); // Gün-saat bilgisi
```

### 3. Derse Kayýt Test

```javascript
const result = await api.post('/course-selection/enroll', {
  courseId: 3,
  sectionCode: 'A',
  semester: 1
});
console.log(result.data.schedule); // Eklenen dersin saatleri
```

### 4. Programý Görüntüle

```javascript
const mySchedule = await api.get('/student-courses/my-schedule?semester=1');
console.log(mySchedule.data.weeklySchedule.Pazartesi); // Pazartesi dersleri
```

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-07  
**Durum:** ? Production Ready

**Endpoint'ler:**
- ? `/api/course-selection/available` - Ders seçimi listesi
- ? `/api/course-selection/enroll` - Derse kayýt
- ? `/api/course-selection/unenroll` - Dersten çýk
- ? `/api/student-courses/my-schedule` - Ders programým

?? **Frontend artýk ders programý sekmesinden schedule bilgisiyle ders ekleyebilir!**
