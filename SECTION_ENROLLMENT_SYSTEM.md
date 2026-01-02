# ?? Course Section System - Implementation Complete

**Date:** 2025-01-06
**Feature:** Section-based course enrollment with automatic conflict detection  
**Status:** ? IMPLEMENTED

---

## ?? System Overview

### Section-Based Enrollment
Each course has multiple sections (A, B, C) with different time slots. Students must choose ONE section per course.

**Example: BÝL101**
- **Section A:** Monday 09:00-11:00, Wednesday 13:00-15:00
- **Section B:** Tuesday 10:00-12:00, Thursday 14:00-16:00
- **Section C:** Monday 13:00-15:00, Friday 09:00-11:00

Student picks ONE section (e.g., Section A) and gets all sessions of that section.

---

## ??? Database Structure

### CourseSchedule (Updated)
```sql
CREATE TABLE CourseSchedules (
    Id INT PRIMARY KEY,
    CourseId INT NOT NULL,
    Semester INT NOT NULL,
    SectionCode NVARCHAR(10) NOT NULL,    -- "A", "B", "C"
    SessionNumber INT NOT NULL,             -- 1, 2, 3, 4
    DayOfWeek INT NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsTheory BIT NOT NULL,
    MaxCapacity INT NOT NULL DEFAULT 50,
    RoomNumber NVARCHAR(50),
    InstructorName NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL
)
```

**Example Data for BÝL101 Section A:**
| Session | Day | Start | End | Type |
|---------|-----|-------|-----|------|
| 1 | Monday | 09:00 | 11:00 | Theory |
| 2 | Wednesday | 13:00 | 15:00 | Theory |
| 3 | Thursday | 10:00 | 11:00 | Practice |

### StudentCourseSection (New)
```sql
CREATE TABLE StudentCourseSections (
    Id INT PRIMARY KEY,
    StudentId NVARCHAR(450) NOT NULL,
    CourseId INT NOT NULL,
    SectionCode NVARCHAR(10) NOT NULL,
    Semester INT NOT NULL,
    EnrolledAt DATETIME2 NOT NULL,
    IsCompleted BIT NOT NULL,
    Grade FLOAT NULL,
    LetterGrade NVARCHAR(MAX) NULL,
    CompletionDate DATETIME2 NULL,
  
    CONSTRAINT FK_StudentCourseSection_Course 
        FOREIGN KEY (CourseId) REFERENCES Courses(Id),
    CONSTRAINT UK_Student_Course_Semester 
        UNIQUE (StudentId, CourseId, Semester)
)
```

---

## ?? API Endpoints

### 1. Get Available Sections
```http
GET /api/section-enrollment/available-sections/{courseId}/{semester}
Authorization: Bearer {token}
```

**Example:**
```http
GET /api/section-enrollment/available-sections/5/1
```

**Response:**
```json
{
  "courseId": 5,
  "courseCode": "BÝL101",
  "courseName": "BÝLGÝSAYAR YAZILIMI I",
  "semester": 1,
  "totalSections": 3,
  "sections": [
    {
      "sectionCode": "A",
      "sessions": [
        {
     "id": 1,
    "sessionNumber": 1,
       "day": "Monday",
     "startTime": "09:00",
      "endTime": "11:00",
          "duration": "120 min",
     "isTheory": true,
          "roomNumber": "B201",
          "instructorName": "Prof. Dr. Ahmet Yýlmaz",
          "maxCapacity": 50
   },
     {
      "id": 2,
          "sessionNumber": 2,
 "day": "Wednesday",
     "startTime": "13:00",
 "endTime": "15:00",
          "duration": "120 min",
        "isTheory": false,
   "roomNumber": "Lab-1",
    "instructorName": "Arþ. Gör. Ayþe Demir",
     "maxCapacity": 50
      }
      ],
      "totalSessions": 2,
      "enrolledCount": 35,
      "isFull": false
    },
    {
      "sectionCode": "B",
      "sessions": [...],
      "totalSessions": 2,
      "enrolledCount": 50,
      "isFull": true
    }
  ]
}
```

---

### 2. Enroll in Section
```http
POST /api/section-enrollment/enroll
Authorization: Bearer {student-token}
Content-Type: application/json

{
"courseId": 5,
  "sectionCode": "A",
  "semester": 1
}
```

**Validations:**
- ? Course exists
- ? Section exists
- ? Not already enrolled in ANY section of this course
- ? Section not full
- ? No time conflicts with other enrolled courses

**Response (Success):**
```json
{
  "message": "Enrolled successfully",
  "enrollmentId": 123,
  "sectionCode": "A"
}
```

**Error Responses:**

Already Enrolled:
```json
{
  "error": "Already enrolled in section B"
}
```

Section Full:
```json
{
  "error": "Section is full"
}
```

Time Conflict:
```json
{
  "error": "Time conflict",
  "details": "Conflicts with MAT151 on Monday"
}
```

---

### 3. Get My Schedule
```http
GET /api/section-enrollment/my-schedule?semester=1
Authorization: Bearer {student-token}
```

**Response:**
```json
{
  "semester": 1,
  "totalCourses": 5,
  "completedCourses": 0,
  "enrollments": [
    {
      "enrollmentId": 123,
 "courseId": 5,
      "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
   "sectionCode": "A",
    "credits": 3,
      "ects": 5,
      "isCompleted": false,
      "grade": null,
      "letterGrade": null,
      "sessions": [
        {
"sessionNumber": 1,
          "day": "Monday",
          "startTime": "09:00",
          "endTime": "11:00",
          "isTheory": true,
        "roomNumber": "B201",
          "instructorName": "Prof. Dr. Ahmet Yýlmaz"
        },
        {
 "sessionNumber": 2,
          "day": "Wednesday",
          "startTime": "13:00",
          "endTime": "15:00",
   "isTheory": false,
  "roomNumber": "Lab-1",
     "instructorName": "Arþ. Gör. Ayþe Demir"
        }
   ]
    }
  ],
  "weeklySchedule": [
    {
      "day": "Monday",
      "courses": [
    {
      "courseCode": "BÝL101",
     "courseName": "BÝLGÝSAYAR YAZILIMI I",
          "sectionCode": "A",
          "day": "Monday",
       "startTime": "09:00",
          "endTime": "11:00",
   "isTheory": true,
          "roomNumber": "B201",
    "instructorName": "Prof. Dr. Ahmet Yýlmaz"
        },
     {
       "courseCode": "MAT151",
  "courseName": "MATEMATÝKSEL ANALÝZ I",
"sectionCode": "B",
     "day": "Monday",
  "startTime": "13:00",
          "endTime": "15:00",
      "isTheory": true,
  "roomNumber": "A101",
          "instructorName": "Prof. Dr. Mehmet Kaya"
        }
      ]
    }
  ]
}
```

---

### 4. Unenroll from Section
```http
DELETE /api/section-enrollment/{enrollmentId}
Authorization: Bearer {student-token}
```

**Response:**
```json
{
  "message": "Unenrolled successfully"
}
```

**Error (Already Completed):**
```json
{
  "error": "Cannot unenroll from completed course"
}
```

---

### 5. Complete Section
```http
PATCH /api/section-enrollment/{enrollmentId}/complete
Authorization: Bearer {student-token}
Content-Type: application/json

{
  "grade": 85.5,
  "letterGrade": "BB",
  "completionDate": "2023-06-15T00:00:00Z"
}
```

**Side Effects:**
- ? Updates StudentProfile.CompletedCredits
- ? Recalculates GPA

**Response:**
```json
{
  "message": "Course completed successfully"
}
```

---

## ?? Student Enrollment Flow

### Step 1: View Available Sections
```javascript
const sections = await api.get(`/section-enrollment/available-sections/5/1`);

// Show sections to student
sections.data.sections.forEach(section => {
  console.log(`Section ${section.sectionCode}:`);
  section.sessions.forEach(session => {
    console.log(`  ${session.day} ${session.startTime}-${session.endTime}`);
  });
  console.log(`Enrolled: ${section.enrolledCount}/${section.sessions[0].maxCapacity}`);
  console.log(`Full: ${section.isFull ? 'Yes' : 'No'}`);
});
```

### Step 2: Choose Section
```javascript
// Student picks Section A
await api.post('/section-enrollment/enroll', {
  courseId: 5,
  sectionCode: 'A',
  semester: 1
});
```

### Step 3: View My Schedule
```javascript
const mySchedule = await api.get('/section-enrollment/my-schedule?semester=1');

// Display weekly schedule
mySchedule.data.weeklySchedule.forEach(day => {
  console.log(`\n${day.day}:`);
  day.courses.forEach(course => {
    console.log(`  ${course.startTime}-${course.endTime}: ${course.courseCode}`);
  });
});
```

---

## ?? Frontend Examples

### React: Section Selection
```jsx
import { useState, useEffect } from 'react';
import api from './api';

const SectionSelector = ({ courseId, semester, onEnroll }) => {
  const [sections, setSections] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSections = async () => {
  try {
        const response = await api.get(
          `/section-enrollment/available-sections/${courseId}/${semester}`
        );
        setSections(response.data.sections);
      } catch (error) {
        console.error('Failed to load sections:', error);
      } finally {
setLoading(false);
      }
    };

    fetchSections();
  }, [courseId, semester]);

const handleEnroll = async (sectionCode) => {
    if (!confirm(`Enroll in Section ${sectionCode}?`)) return;

    try {
      await api.post('/section-enrollment/enroll', {
        courseId,
      sectionCode,
        semester
      });
    
      alert('? Enrolled successfully!');
      onEnroll();
    } catch (error) {
      if (error.response?.status === 400) {
        alert(`? ${error.response.data.error}\n${error.response.data.details || ''}`);
      } else {
  alert('Failed to enroll');
      }
    }
  };

  if (loading) return <div>Loading sections...</div>;

  return (
    <div className="section-selector">
      <h3>Choose a Section</h3>
      
      {sections.map(section => (
        <div key={section.sectionCode} className="section-card">
      <div className="section-header">
            <h4>Section {section.sectionCode}</h4>
            <span className={section.isFull ? 'full' : 'available'}>
    {section.enrolledCount}/{section.sessions[0]?.maxCapacity}
          </span>
          </div>

          <div className="sessions">
            {section.sessions.map(session => (
   <div key={session.sessionNumber} className="session">
   <span className="day">{session.day}</span>
    <span className="time">
      {session.startTime} - {session.endTime}
                </span>
      <span className="type">
      {session.isTheory ? '?? Theory' : '?? Practice'}
    </span>
          {session.roomNumber && (
   <span className="room">Room: {session.roomNumber}</span>
                )}
        </div>
 ))}
     </div>

     <button
            onClick={() => handleEnroll(section.sectionCode)}
            disabled={section.isFull}
          >
            {section.isFull ? 'Full' : 'Enroll'}
          </button>
        </div>
      ))}
    </div>
  );
};

export default SectionSelector;
```

---

### React: My Weekly Schedule
```jsx
const MyWeeklySchedule = ({ semester }) => {
  const [schedule, setSchedule] = useState(null);

  useEffect(() => {
    const fetchSchedule = async () => {
      const response = await api.get(`/section-enrollment/my-schedule?semester=${semester}`);
    setSchedule(response.data);
    };

 fetchSchedule();
  }, [semester]);

  if (!schedule) return <div>Loading...</div>;

  return (
    <div className="weekly-schedule">
      <div className="stats">
        <p>Total Courses: {schedule.totalCourses}</p>
        <p>Completed: {schedule.completedCourses}</p>
      </div>

      <div className="schedule-grid">
        {schedule.weeklySchedule.map(day => (
          <div key={day.day} className="day-column">
      <h3>{day.day}</h3>
     
            {day.courses.map((course, index) => (
           <div key={index} className="course-block">
            <div className="time">
     {course.startTime} - {course.endTime}
            </div>
     <div className="course-code">{course.courseCode}</div>
<div className="section">Section {course.sectionCode}</div>
      {course.roomNumber && (
  <div className="room">{course.roomNumber}</div>
             )}
              </div>
   ))}
          </div>
        ))}
      </div>
    </div>
  );
};
```

---

## ? Key Features

### Automatic Validations
1. ? **One Section Per Course**
   - Student can only enroll in ONE section
   - Already enrolled check

2. ? **Capacity Control**
   - Each section has max capacity (default: 50)
   - Full sections cannot be selected

3. ? **Time Conflict Detection**
   - Automatically checks all enrolled courses
 - Prevents overlapping schedules

4. ? **Session Tracking**
   - 4-hour courses split into sessions
   - All sessions tracked with SessionNumber

5. ? **Grade Management**
   - Complete course with grade
   - Automatic GPA calculation
   - Credit tracking

---

## ?? System Behavior

### Course with 4 Hours (3T + 1U)
**Generated Sections:**
- Section A: Mon 09:00-11:00 (2h), Wed 13:00-15:00 (2h)
- Section B: Tue 10:00-12:00 (2h), Thu 14:00-16:00 (2h)
- Section C: Mon 13:00-15:00 (2h), Fri 09:00-11:00 (2h)

**Student picks Section A:**
- Gets BOTH sessions (Mon + Wed)
- Cannot pick Section B or C for same course
- Can pick other courses if no time conflict

---

## ?? Final Status

**Database:** ? StudentCourseSection table added  
**Validation:** ? All conflict checks implemented  
**API:** ? 5 endpoints ready  
**Features:** ? Section-based enrollment complete  
**Build:** ? Ready (after migration)  
**Production:** ? Needs migration

---

**?? Section-based course enrollment system is complete!** ??

**Next Step:** Apply migration to create `StudentCourseSections` table.

