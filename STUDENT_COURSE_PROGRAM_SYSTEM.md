# ?? Student Course Program System - Complete

**Date:** 2025-01-06  
**Feature:** Student course enrollment and program management  
**Status:** ? IMPLEMENTED

---

## ?? Feature Overview

### Yetkilendirme Kurallarý

| Rol | Yetkiler |
|-----|----------|
| **Student** | ? Sadece kendi ders programýný görüntüleme<br>? Ders kaydý yapma<br>? Ders tamamlama<br>? Kayýt iptali |
| **Advisor** | ? Sadece kendi öðrencilerinin programlarýný görüntüleme |
| **Admin** | ? Tüm öðrenci programlarýný görüntüleme |

---

## ?? API Endpoints

### 1. Get My Course Program (Student)
```http
GET /api/student-courses/my-program
Authorization: Bearer {student-token}
```

**Authorization:** Student (kendi programý)

**Response:**
```json
{
  "totalCourses": 15,
  "completedCourses": 10,
  "totalCredits": 30,
  "totalECTS": 50,
  "gpa": 3.45,
  "courses": [
    {
      "id": 1,
      "semester": 1,
      "courseId": 5,
      "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
      "theoryHours": 3,
      "practiceHours": 1,
      "credits": 3,
      "ects": 5,
  "isElective": false,
      "category": "Birinci Yarýyýl (Güz)",
      "isCompleted": true,
      "grade": 85.5,
   "letterGrade": "BB",
      "completionDate": "2023-06-15T00:00:00Z",
    "enrolledAt": "2023-09-01T00:00:00Z"
    }
  ]
}
```

---

### 2. Get Student Program (Advisor/Admin)
```http
GET /api/student-courses/student/{studentId}
Authorization: Bearer {advisor-token}
```

**Authorization:** 
- Advisor: Sadece kendi öðrencileri
- Admin: Tüm öðrenciler

**Response:**
```json
{
  "studentId": "student-id-123",
  "studentName": "student1@local",
  "studentEmail": "student1@local",
  "totalCourses": 15,
  "completedCourses": 10,
  "totalCredits": 30,
  "totalECTS": 50,
  "gpa": 3.45,
  "courses": [
    {
      "id": 1,
      "semester": 1,
      "courseId": 5,
      "courseCode": "BÝL101",
      "courseName": "BÝLGÝSAYAR YAZILIMI I",
      "theoryHours": 3,
      "practiceHours": 1,
      "credits": 3,
      "ects": 5,
      "isElective": false,
      "category": "Birinci Yarýyýl (Güz)",
    "isCompleted": true,
      "grade": 85.5,
      "letterGrade": "BB",
      "completionDate": "2023-06-15T00:00:00Z",
      "enrolledAt": "2023-09-01T00:00:00Z"
    }
  ]
}
```

**Error (403 - Advisor trying to access other advisor's student):**
```json
{
  "error": "Forbidden"
}
```

---

### 3. Enroll in Course
```http
POST /api/student-courses/enroll
Authorization: Bearer {student-token}
Content-Type: application/json

{
  "courseId": 5,
  "semester": 1
}
```

**Authorization:** Student

**Response:**
```json
{
  "message": "Enrolled successfully",
  "enrollmentId": 1
}
```

**Error Responses:**

404 Not Found:
```json
{
  "error": "Course not found"
}
```

400 Bad Request:
```json
{
  "error": "Already enrolled in this course"
}
```

---

### 4. Complete Course
```http
PATCH /api/student-courses/{enrollmentId}/complete
Authorization: Bearer {student-token}
Content-Type: application/json

{
  "grade": 85.5,
  "letterGrade": "BB",
  "completionDate": "2023-06-15T00:00:00Z"
}
```

**Authorization:** Student (own enrollments only)

**Response:**
```json
{
  "message": "Course completed successfully"
}
```

**Side Effects:**
- ? Updates student profile credits
- ? Recalculates GPA
- ? Updates completion status

---

### 5. Unenroll from Course
```http
DELETE /api/student-courses/{enrollmentId}
Authorization: Bearer {student-token}
```

**Authorization:** Student (own enrollments only)

**Response:**
```json
{
  "message": "Unenrolled successfully"
}
```

**Error (400 - Already completed):**
```json
{
  "error": "Cannot unenroll from completed course"
}
```

---

## ?? Authorization Logic

### GetMyProgram (Student)
```csharp
// ? Student only sees own program
var userId = GetUserId();
var courses = await _db.StudentCourses
    .Where(sc => sc.StudentId == userId)
    ...
```

### GetStudentProgram (Advisor)
```csharp
// ? Advisor can only see own students
var student = await _userManager.FindByIdAsync(studentId);
if (isAdvisor && !isAdmin && student.AdvisorId != currentUserId)
    return Forbid();
```

### GetStudentProgram (Admin)
```csharp
// ? Admin can see all students
if (isAdmin)
{
    // No restrictions
}
```

---

## ?? Automatic Calculations

### GPA Calculation
```csharp
var allGrades = await _db.StudentCourses
    .Where(sc => sc.StudentId == userId && sc.Grade.HasValue)
    .Select(sc => sc.Grade!.Value)
    .ToListAsync();

if (allGrades.Any())
{
    profile.GPA = Math.Round(allGrades.Average(), 2);
}
```

### Credit Calculation
```csharp
var completedCredits = await _db.StudentCourses
    .Where(sc => sc.StudentId == userId && sc.IsCompleted)
.Include(sc => sc.Course)
    .SumAsync(sc => sc.Course.Credits);

profile.CompletedCredits = completedCredits;
```

---

## ?? Frontend Examples

### React: Display My Program
```jsx
import { useEffect, useState } from 'react';
import api from './api';

const MyProgram = () => {
  const [program, setProgram] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProgram = async () => {
 try {
        const response = await api.get('/student-courses/my-program');
        setProgram(response.data);
      } catch (error) {
        console.error('Failed to load program:', error);
      } finally {
    setLoading(false);
      }
    };

    fetchProgram();
  }, []);

if (loading) return <div>Loading...</div>;

  return (
    <div className="my-program">
      <div className="stats">
    <h2>My Course Program</h2>
        <div className="stats-grid">
          <div className="stat">
     <span>Total Courses</span>
         <strong>{program.totalCourses}</strong>
          </div>
          <div className="stat">
  <span>Completed</span>
          <strong>{program.completedCourses}</strong>
          </div>
 <div className="stat">
            <span>Credits</span>
     <strong>{program.totalCredits}</strong>
          </div>
   <div className="stat">
  <span>ECTS</span>
 <strong>{program.totalECTS}</strong>
      </div>
          <div className="stat">
      <span>GPA</span>
            <strong>{program.gpa?.toFixed(2) || 'N/A'}</strong>
    </div>
   </div>
      </div>

      <div className="courses-by-semester">
    {[1, 2, 3, 4, 5, 6, 7, 8].map(sem => {
     const semesterCourses = program.courses.filter(c => c.semester === sem);
       if (semesterCourses.length === 0) return null;

          return (
    <div key={sem} className="semester-section">
      <h3>Semester {sem}</h3>
         <table>
           <thead>
     <tr>
     <th>Code</th>
            <th>Course</th>
         <th>T</th>
         <th>U</th>
        <th>K</th>
        <th>ECTS</th>
         <th>Grade</th>
       <th>Status</th>
        </tr>
     </thead>
          <tbody>
       {semesterCourses.map(course => (
  <tr key={course.id} className={course.isCompleted ? 'completed' : 'in-progress'}>
     <td>{course.courseCode}</td>
  <td>{course.courseName}</td>
    <td>{course.theoryHours}</td>
     <td>{course.practiceHours}</td>
   <td>{course.credits}</td>
<td>{course.ects}</td>
   <td>
       {course.grade ? (
  <span>{course.grade} ({course.letterGrade})</span>
          ) : '-'}
     </td>
      <td>
         {course.isCompleted ? (
               <span className="badge success">? Completed</span>
    ) : (
       <span className="badge pending">In Progress</span>
                  )}
   </td>
          </tr>
              ))}
      </tbody>
       </table>
            </div>
          );
     })}
      </div>
    </div>
  );
};

export default MyProgram;
```

---

### React: Advisor View Student Program
```jsx
const StudentProgram = ({ studentId }) => {
  const [program, setProgram] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchProgram = async () => {
      try {
        const response = await api.get(`/student-courses/student/${studentId}`);
   setProgram(response.data);
      } catch (error) {
        if (error.response?.status === 403) {
          setError('You can only view your own students\' programs');
        } else {
          setError('Failed to load student program');
        }
      } finally {
        setLoading(false);
      }
    };

    fetchProgram();
  }, [studentId]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="student-program">
      <div className="student-info">
        <h2>{program.studentName}</h2>
        <p>{program.studentEmail}</p>
      </div>

    <div className="stats">
        <div className="stat">
          <span>Total Courses:</span>
          <strong>{program.totalCourses}</strong>
        </div>
   <div className="stat">
    <span>Completed:</span>
          <strong>{program.completedCourses}</strong>
        </div>
        <div className="stat">
    <span>Credits:</span>
   <strong>{program.totalCredits}</strong>
     </div>
    <div className="stat">
   <span>GPA:</span>
   <strong>{program.gpa?.toFixed(2) || 'N/A'}</strong>
        </div>
      </div>

    {/* Course list... */}
    </div>
  );
};
```

---

### React: Enroll in Course
```jsx
const EnrollButton = ({ courseId, semester }) => {
  const [enrolling, setEnrolling] = useState(false);

  const handleEnroll = async () => {
    setEnrolling(true);
    try {
      await api.post('/student-courses/enroll', {
        courseId,
        semester
      });
      alert('? Enrolled successfully!');
      window.location.reload(); // or update state
    } catch (error) {
      if (error.response?.status === 400) {
 alert('Already enrolled in this course');
      } else {
        alert('Failed to enroll');
      }
    } finally {
      setEnrolling(false);
    }
  };

  return (
    <button onClick={handleEnroll} disabled={enrolling}>
      {enrolling ? 'Enrolling...' : 'Enroll'}
    </button>
  );
};
```

---

### React: Complete Course
```jsx
const CompleteCourseForm = ({ enrollmentId }) => {
  const [formData, setFormData] = useState({
    grade: '',
    letterGrade: '',
    completionDate: new Date().toISOString().split('T')[0]
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      await api.patch(`/student-courses/${enrollmentId}/complete`, {
        grade: parseFloat(formData.grade),
        letterGrade: formData.letterGrade,
     completionDate: new Date(formData.completionDate).toISOString()
      });
      
  alert('? Course completed!');
      window.location.reload();
    } catch (error) {
      alert('Failed to complete course');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
  <input
 type="number"
      step="0.01"
 min="0"
    max="100"
   placeholder="Grade (0-100)"
   value={formData.grade}
        onChange={e => setFormData({...formData, grade: e.target.value})}
        required
      />
      
      <select
        value={formData.letterGrade}
        onChange={e => setFormData({...formData, letterGrade: e.target.value})}
        required
 >
        <option value="">Select Grade</option>
        <option value="AA">AA</option>
  <option value="BA">BA</option>
        <option value="BB">BB</option>
  <option value="CB">CB</option>
        <option value="CC">CC</option>
      <option value="DC">DC</option>
        <option value="DD">DD</option>
        <option value="FD">FD</option>
        <option value="FF">FF</option>
    </select>

      <input
        type="date"
     value={formData.completionDate}
        onChange={e => setFormData({...formData, completionDate: e.target.value})}
 required
      />

      <button type="submit">Complete Course</button>
    </form>
  );
};
```

---

## ? Implementation Summary

### Database
- [x] Updated StudentCourse entity
- [x] Migration created and applied
- [x] Foreign key to Course table
- [x] Automatic GPA calculation
- [x] Automatic credit calculation

### API Endpoints
- [x] GET /api/student-courses/my-program (Student)
- [x] GET /api/student-courses/student/{id} (Advisor/Admin)
- [x] POST /api/student-courses/enroll (Student)
- [x] PATCH /api/student-courses/{id}/complete (Student)
- [x] DELETE /api/student-courses/{id} (Student)

### Authorization
- [x] Students see only own program
- [x] Advisors see only own students
- [x] Admins see all students
- [x] Proper 403 errors

### Features
- [x] Course enrollment
- [x] Grade tracking
- [x] GPA calculation
- [x] Credit tracking
- [x] Semester organization
- [x] Completion tracking

---

## ?? Final Status

**Database:** ? Updated  
**Migration:** ? Applied  
**API:** ? 5 Endpoints  
**Authorization:** ? Role-based (v3.1)  
**Build:** ? Successful  
**Ready for:** ? Production

---

**?? Student course program system is complete and ready to use!** ??

