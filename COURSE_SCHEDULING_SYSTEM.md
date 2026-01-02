# ?? Course Scheduling System - Complete

**Date:** 2025-01-06  
**Feature:** Automatic course timetable generation with conflict detection  
**Status:** ? IMPLEMENTED

---

## ?? Feature Overview

### Intelligent Course Scheduling
- ? **Automatic timetable generation** for each semester
- ? **Smart hour distribution:** 4 hours ? 2+2 sessions
- ? **Conflict detection:** Same time slots blocked
- ? **Weekly schedule view:** Monday-Friday grid
- ? **Manual adjustments:** Edit individual schedules

---

## ?? Scheduling Algorithm

### Time Slots (Monday-Friday)
```
09:00 - 10:00
10:00 - 11:00
11:00 - 12:00
13:00 - 14:00 (after lunch break)
14:00 - 15:00
15:00 - 16:00
16:00 - 17:00
```

### Hour Distribution Rules
| Total Hours | Sessions | Example |
|-------------|----------|---------|
| 1 hour | 1 × 1h | BÝL300 (STAJ I) |
| 2 hours | 1 × 2h | BÝL110 (2T+0U) |
| 3 hours | 2h + 1h | MAT151 (4T+1U split) |
| 4 hours | 2h + 2h | BÝL324 (3T+2U=5 ? 2+2) |
| 5 hours | 2h + 2h + 1h | BÝL324 (3T+2U) |

**Smart Splitting:**
- 4 saat olacurses automatically split into 2+2
- Theory and practice hours considered
- Consecutive slots allocated together

---

## ??? Database Tables

### CourseSchedule
```sql
CREATE TABLE CourseSchedules (
    Id INT PRIMARY KEY,
    CourseId INT NOT NULL,
    Semester INT NOT NULL,
    DayOfWeek INT NOT NULL,      -- 1=Monday, 5=Friday
    StartTime TIME NOT NULL,       -- e.g., 09:00:00
    EndTime TIME NOT NULL,         -- e.g., 11:00:00
    RoomNumber NVARCHAR(50),
    InstructorName NVARCHAR(MAX),
    IsTheory BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
)
```

### ScheduleConflict
```sql
CREATE TABLE ScheduleConflicts (
 Id INT PRIMARY KEY,
 Schedule1Id INT NOT NULL,
    Schedule2Id INT NOT NULL,
    ConflictType NVARCHAR(MAX),
    Description NVARCHAR(MAX),
    DetectedAt DATETIME2 NOT NULL
)
```

---

## ?? API Endpoints

### 1. Generate Schedule (Admin Only)
```http
POST /api/schedule/generate/{semester}
Authorization: Bearer {admin-token}
```

**Example:**
```http
POST /api/schedule/generate/1
```

**Response:**
```json
{
  "message": "Schedule generated for semester 1",
  "totalSchedules": 24,
  "conflicts": 0,
  "schedule": [
    {
      "id": 1,
      "courseId": 5,
      "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
   "dayOfWeek": "Monday",
 "startTime": "09:00",
      "endTime": "11:00",
   "isTheory": true,
      "roomNumber": null,
      "instructorName": null
    },
 {
      "id": 2,
      "courseId": 5,
      "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
    "dayOfWeek": "Wednesday",
      "startTime": "13:00",
      "endTime": "15:00",
      "isTheory": false,
      "roomNumber": null,
      "instructorName": null
    }
  ]
}
```

---

### 2. Get Schedule by Semester
```http
GET /api/schedule/semester/{semester}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "semester": 1,
  "totalSchedules": 24,
  "byDay": [
  {
      "day": "Monday",
      "courses": [
        {
          "id": 1,
          "courseId": 5,
       "courseCode": "BÝL101",
        "courseName": "BÝLGÝSAYAR YAZILIMI I",
          "theoryHours": 3,
 "practiceHours": 1,
     "credits": 3,
    "ects": 5,
 "dayOfWeek": "Monday",
          "startTime": "09:00",
     "endTime": "11:00",
          "durationMinutes": 120,
          "isTheory": true,
          "roomNumber": null,
          "instructorName": null,
        "category": "Birinci Yarýyýl (Güz)"
        }
      ]
    }
],
  "allSchedules": [...]
}
```

---

### 3. Get Weekly Schedule Grid
```http
GET /api/schedule/week/{semester}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "semester": 1,
  "weeklySchedule": [
    {
      "day": "Monday",
      "courses": [
      {
          "id": 1,
   "courseCode": "BÝL101",
          "courseName": "BÝLGÝSAYAR YAZILIMI I",
     "startTime": "09:00",
"endTime": "11:00",
          "duration": "120 min",
        "type": "Theory",
  "roomNumber": null,
          "instructorName": null
    },
        {
      "id": 5,
          "courseCode": "MAT151",
     "courseName": "MATEMATÝKSEL ANALÝZ I",
        "startTime": "13:00",
      "endTime": "15:00",
          "duration": "120 min",
          "type": "Theory",
 "roomNumber": null,
   "instructorName": null
        }
      ]
    },
    {
    "day": "Tuesday",
  "courses": [...]
    }
  ]
}
```

---

### 4. Detect Conflicts (Admin Only)
```http
GET /api/schedule/conflicts/{semester}
Authorization: Bearer {admin-token}
```

**Response (No Conflicts):**
```json
{
  "semester": 1,
  "totalConflicts": 0,
  "conflicts": []
}
```

**Response (With Conflicts):**
```json
{
  "semester": 1,
  "totalConflicts": 1,
  "conflicts": [
    {
      "conflictType": "TimeOverlap",
      "description": "BÝL101 and MAT151 overlap on Monday",
      "course1": {
        "courseCode": "BÝL101",
        "courseName": "BÝLGÝSAYAR YAZILIMI I",
        "day": "Monday",
        "startTime": "09:00",
        "endTime": "11:00"
      },
      "course2": {
        "courseCode": "MAT151",
        "courseName": "MATEMATÝKSEL ANALÝZ I",
        "day": "Monday",
        "startTime": "10:00",
        "endTime": "12:00"
      },
      "detectedAt": "2025-01-06T12:00:00Z"
    }
  ]
}
```

---

### 5. Update Schedule (Admin Only)
```http
PUT /api/schedule/{scheduleId}
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "dayOfWeek": 2,  // Tuesday
  "startTime": "13:00:00",
  "endTime": "15:00:00",
  "roomNumber": "B201",
  "instructorName": "Prof. Dr. Ahmet Yýlmaz"
}
```

**Response:**
```json
{
  "message": "Schedule updated successfully"
}
```

**Error (Conflict):**
```json
{
  "error": "Schedule update would create a conflict"
}
```

---

### 6. Delete Schedule (Admin Only)
```http
DELETE /api/schedule/semester/{semester}
Authorization: Bearer {admin-token}
```

**Response:**
```json
{
  "message": "Schedule for semester 1 deleted",
  "deletedCount": 24
}
```

---

## ?? Algorithm Details

### Generation Process

1. **Load Courses**
   - Get all required courses for semester
   - Sort by total weekly hours (descending)
   - Prioritize courses with more hours

2. **Split Hours into Sessions**
   ```csharp
   4 hours ? [2, 2]
   3 hours ? [2, 1]
   2 hours ? [2]
   1 hour ? [1]
   ```

3. **Assign to Time Slots**
   - Try each day (Monday-Friday)
   - Try each time slot (09:00-16:00)
   - Check if slot is available
   - Check for consecutive slots (if needed)
   - Mark slots as used

4. **Detect Conflicts**
   - Compare all schedules
- Check time overlaps
   - Report conflicts

### Conflict Detection Logic
```csharp
bool IsConflict(Schedule s1, Schedule s2) =>
    s1.DayOfWeek == s2.DayOfWeek &&
    s1.StartTime < s2.EndTime &&
    s2.StartTime < s1.EndTime;
```

---

## ?? Frontend Examples

### React: Display Weekly Schedule
```jsx
import { useEffect, useState } from 'react';
import api from './api';

const WeeklySchedule = ({ semester }) => {
  const [schedule, setSchedule] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSchedule = async () => {
      try {
        const response = await api.get(`/schedule/week/${semester}`);
     setSchedule(response.data);
      } catch (error) {
        console.error('Failed to load schedule:', error);
      } finally {
      setLoading(false);
  }
    };

    fetchSchedule();
  }, [semester]);

  if (loading) return <div>Loading...</div>;

  return (
    <div className="weekly-schedule">
    <h2>Semester {semester} - Weekly Schedule</h2>
      
      <div className="schedule-grid">
        {schedule.weeklySchedule.map(day => (
      <div key={day.day} className="day-column">
   <h3>{day.day}</h3>
      
            {day.courses.length === 0 ? (
              <div className="no-classes">No classes</div>
            ) : (
    day.courses.map(course => (
   <div key={course.id} className="course-card">
        <div className="time">{course.startTime} - {course.endTime}</div>
     <div className="code">{course.courseCode}</div>
        <div className="name">{course.courseName}</div>
             <div className="type">{course.type}</div>
             {course.roomNumber && (
       <div className="room">Room: {course.roomNumber}</div>
       )}
      {course.instructorName && (
             <div className="instructor">{course.instructorName}</div>
             )}
       </div>
         ))
   )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default WeeklySchedule;
```

---

### React: Generate Schedule (Admin)
```jsx
const ScheduleGenerator = () => {
  const [semester, setSemester] = useState(1);
  const [generating, setGenerating] = useState(false);
  const [result, setResult] = useState(null);

  const handleGenerate = async () => {
  if (!confirm(`Generate schedule for semester ${semester}? This will replace existing schedule.`)) {
      return;
    }

    setGenerating(true);
    try {
      const response = await api.post(`/schedule/generate/${semester}`);
      setResult(response.data);
    
      if (response.data.conflicts > 0) {
        alert(`?? Warning: ${response.data.conflicts} conflicts detected!`);
      } else {
    alert('? Schedule generated successfully!');
      }
    } catch (error) {
   alert('Failed to generate schedule');
    } finally {
 setGenerating(false);
    }
  };

  return (
    <div className="schedule-generator">
      <h2>Generate Course Schedule</h2>
      
      <select value={semester} onChange={e => setSemester(parseInt(e.target.value))}>
        {[1, 2, 3, 4, 5, 6, 7, 8].map(sem => (
          <option key={sem} value={sem}>Semester {sem}</option>
  ))}
      </select>

      <button onClick={handleGenerate} disabled={generating}>
        {generating ? 'Generating...' : 'Generate Schedule'}
      </button>

   {result && (
        <div className="result">
      <h3>Result:</h3>
          <p>Total Schedules: {result.totalSchedules}</p>
          <p>Conflicts: {result.conflicts}</p>
          
      {result.conflicts > 0 && (
   <button onClick={() => window.location.href = `/schedule/conflicts/${semester}`}>
View Conflicts
            </button>
   )}
        </div>
      )}
  </div>
  );
};
```

---

### React: View Conflicts
```jsx
const ConflictViewer = ({ semester }) => {
  const [conflicts, setConflicts] = useState([]);

  useEffect(() => {
    const fetchConflicts = async () => {
      const response = await api.get(`/schedule/conflicts/${semester}`);
      setConflicts(response.data.conflicts);
    };

    fetchConflicts();
  }, [semester]);

  if (conflicts.length === 0) {
    return <div className="success">? No conflicts found!</div>;
  }

  return (
    <div className="conflicts">
      <h3>?? Schedule Conflicts</h3>
      
      {conflicts.map((conflict, index) => (
        <div key={index} className="conflict-card">
          <h4>{conflict.conflictType}</h4>
          <p>{conflict.description}</p>
  
          <div className="courses">
        <div className="course">
       <strong>{conflict.course1.courseCode}</strong>
     <div>{conflict.course1.day} {conflict.course1.startTime}-{conflict.course1.endTime}</div>
       </div>
            
   <span className="vs">VS</span>
       
            <div className="course">
    <strong>{conflict.course2.courseCode}</strong>
    <div>{conflict.course2.day} {conflict.course2.startTime}-{conflict.course2.endTime}</div>
   </div>
          </div>
    </div>
      ))}
    </div>
  );
};
```

---

## ? Implementation Summary

### Database
- [x] 2 new tables created
- [x] Migration applied
- [x] Indexes for performance

### Service
- [x] Intelligent scheduling algorithm
- [x] Conflict detection
- [x] Session splitting (4h ? 2+2)
- [x] Time slot management

### API
- [x] 6 endpoints
- [x] Admin controls
- [x] Weekly view
- [x] Conflict reporting

### Features
- [x] Automatic generation
- [x] No time conflicts
- [x] Smart hour distribution
- [x] Manual adjustments
- [x] Room/instructor assignment

---

## ?? Final Status

**Database:** ? Tables created  
**Migration:** ? Applied  
**Service:** ? ICourseScheduler implemented  
**Controller:** ? 6 endpoints  
**Algorithm:** ? Conflict-free scheduling  
**Build:** ? Successful  
**Ready for:** ? Production

---

**?? Automatic course scheduling system is complete and ready!** ??

