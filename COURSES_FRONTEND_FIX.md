# ?? Courses Page - Frontend Fix Guide

**Issue:** Frontend getting 404 errors on courses page  
**Date:** 2025-01-07  
**Status:** ? Fix Available

---

## ? Problem

Frontend is calling **wrong endpoints**:

```javascript
// ? WRONG - Returns 404
GET /api/course/requirements
GET /api/course/my-courses
```

**Console Errors:**
```
Failed to load resource: the server responded with a status of 404 ()
? API Error: Object
Failed to load data: AxiosError
```

---

## ? Solution

Use the **correct endpoints**:

```javascript
// ? CORRECT - Returns all courses
GET /api/courses

// ? CORRECT - Returns student's enrolled courses
GET /api/student-courses/my-program
```

---

## ?? Available Endpoints

### 1. Get All Courses (Public - All Roles)

```http
GET /api/courses?categoryId=1&semester=3&isElective=false&search=matematik
Authorization: Bearer {token}
```

**Query Parameters (All Optional):**
- `categoryId` (int) - Filter by category
- `semester` (int) - Filter by semester (1-8)
- `isElective` (bool) - Filter elective/required courses
- `search` (string) - Search in course code or name

**Response:**
```json
{
  "totalCount": 45,
  "courses": [
    {
  "id": 1,
      "courseCode": "BÝL101",
      "courseName": "Bilgisayar Bilimine Giriþ",
      "theoryHours": 3,
      "practiceHours": 2,
      "credits": 4,
      "ects": 6,
      "semester": 1,
      "isElective": false,
      "description": "Bilgisayar biliminin temel kavramlarý...",
      "category": {
        "id": 1,
        "name": "Temel Bilgisayar Bilimleri",
        "displayOrder": 1
      }
    }
  ]
}
```

---

### 2. Get Course by ID

```http
GET /api/courses/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 1,
  "courseCode": "BÝL101",
  "courseName": "Bilgisayar Bilimine Giriþ",
  "theoryHours": 3,
  "practiceHours": 2,
  "credits": 4,
  "ects": 6,
  "semester": 1,
  "isElective": false,
  "description": "Bilgisayar biliminin temel kavramlarý ve programlama temelleri...",
  "category": {
    "id": 1,
    "name": "Temel Bilgisayar Bilimleri"
  },
  "prerequisites": [
    {
      "id": 1,
      "prerequisiteCourseId": 2,
      "courseCode": "MAT101",
      "courseName": "Matematik I",
      "isMandatory": true
}
  ]
}
```

---

### 3. Get Categories

```http
GET /api/courses/categories
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Temel Bilgisayar Bilimleri",
    "description": "Temel bilgisayar bilimleri dersleri",
    "displayOrder": 1,
    "courseCount": 12
  },
  {
    "id": 2,
 "name": "Yazýlým Geliþtirme",
    "description": "Yazýlým geliþtirme ve mühendislik dersleri",
    "displayOrder": 2,
    "courseCount": 15
  }
]
```

---

### 4. Get Courses by Semester

```http
GET /api/courses/by-semester/3
Authorization: Bearer {token}
```

**Response:**
```json
{
  "semester": 3,
  "totalCourses": 8,
  "requiredCourses": 6,
  "electiveCourses": 2,
  "totalCredits": 28,
  "totalECTS": 30,
  "courses": [
    {
      "id": 15,
    "courseCode": "BÝL301",
      "courseName": "Veri Yapýlarý",
   "theoryHours": 3,
    "practiceHours": 2,
  "credits": 4,
      "ects": 6,
      "isElective": false,
    "category": "Temel Bilgisayar Bilimleri"
    }
  ]
}
```

---

### 5. Get Elective Courses

```http
GET /api/courses/electives
Authorization: Bearer {token}
```

**Response:**
```json
{
  "totalElectives": 20,
  "electives": [
    {
      "id": 45,
      "courseCode": "BÝL401",
      "courseName": "Yapay Zeka",
      "theoryHours": 3,
    "practiceHours": 0,
      "credits": 3,
      "ects": 5,
      "category": "Ýleri Konular"
    }
  ]
}
```

---

### 6. Get My Enrolled Courses (Student)

```http
GET /api/student-courses/my-program
Authorization: Bearer {token}
```

**Response:**
```json
{
  "totalCourses": 25,
  "completedCourses": 20,
  "totalCredits": 80,
  "totalECTS": 120,
  "gpa": 3.45,
  "courses": [
  {
      "id": 1,
      "semester": 1,
      "courseId": 1,
      "courseCode": "BÝL101",
      "courseName": "Bilgisayar Bilimine Giriþ",
  "theoryHours": 3,
      "practiceHours": 2,
      "credits": 4,
      "ects": 6,
   "isElective": false,
  "category": "Temel Bilgisayar Bilimleri",
      "isCompleted": true,
      "grade": 85.5,
      "letterGrade": "AA",
 "completionDate": "2024-06-15T00:00:00Z",
      "enrolledAt": "2024-02-01T00:00:00Z"
    }
  ]
}
```

---

## ?? Frontend Implementation

### Courses.jsx - Fix

**? Old (Broken) Code:**
```javascript
// WRONG endpoints
const loadData = async () => {
  try {
 const requirements = await api.get('/course/requirements');  // 404!
    const myCourses = await api.get('/course/my-courses');  // 404!
    
    setRequirements(requirements.data);
    setMyCourses(myCourses.data);
  } catch (error) {
    console.error('Failed to load data:', error);
  }
};
```

**? New (Fixed) Code:**
```javascript
// CORRECT endpoints
const loadData = async () => {
  try {
    // Load all available courses
    const allCourses = await api.get('/courses');
  setCourses(allCourses.data.courses);
    
    // Load categories for filtering
    const categories = await api.get('/courses/categories');
    setCategories(categories.data);
    
  // If student, load enrolled courses
    if (userRole === 'Student') {
      const myProgram = await api.get('/student-courses/my-program');
   setMyProgram(myProgram.data);
    }
  } catch (error) {
    console.error('Failed to load data:', error);
    toast.error('Dersler yüklenemedi');
  }
};
```

---

### Complete Courses Page Component

```javascript
import { useState, useEffect } from 'react';
import api from '../services/api';
import { toast } from 'react-toastify';

const CoursesPage = () => {
  const [courses, setCourses] = useState([]);
  const [categories, setCategories] = useState([]);
  const [myProgram, setMyProgram] = useState(null);
  const [loading, setLoading] = useState(true);
  
  // Filters
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [selectedSemester, setSelectedSemester] = useState(null);
  const [showElectivesOnly, setShowElectivesOnly] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  
  const userRole = localStorage.getItem('role'); // 'Student', 'Advisor', 'Admin'

  useEffect(() => {
    loadData();
  }, [selectedCategory, selectedSemester, showElectivesOnly, searchTerm]);

  const loadData = async () => {
    setLoading(true);
    try {
      // Build query parameters
      const params = new URLSearchParams();
      if (selectedCategory) params.append('categoryId', selectedCategory);
      if (selectedSemester) params.append('semester', selectedSemester);
   if (showElectivesOnly) params.append('isElective', 'true');
      if (searchTerm) params.append('search', searchTerm);

  // Load courses with filters
  const response = await api.get(`/courses?${params.toString()}`);
      setCourses(response.data.courses);

      // Load categories (once)
      if (categories.length === 0) {
        const categoriesResponse = await api.get('/courses/categories');
        setCategories(categoriesResponse.data);
      }

      // If student, load enrolled courses
   if (userRole === 'Student' && !myProgram) {
        const programResponse = await api.get('/student-courses/my-program');
        setMyProgram(programResponse.data);
      }
    } catch (error) {
      console.error('Failed to load courses:', error);
      toast.error('Dersler yüklenemedi');
    } finally {
 setLoading(false);
    }
  };

  const isEnrolled = (courseId) => {
    if (!myProgram) return false;
    return myProgram.courses.some(c => c.courseId === courseId);
  };

  const getCourseStatus = (courseId) => {
    if (!myProgram) return null;
    const course = myProgram.courses.find(c => c.courseId === courseId);
    return course?.isCompleted ? 'completed' : 'enrolled';
  };

  return (
    <div className="courses-page">
      <div className="page-header">
        <h1>?? Dersler</h1>
        <p>Tüm ders programý ve detaylarý</p>
      </div>

      {/* Student Stats */}
      {userRole === 'Student' && myProgram && (
        <div className="student-stats">
          <div className="stat-card">
        <span className="stat-label">Toplam Ders</span>
       <span className="stat-value">{myProgram.totalCourses}</span>
          </div>
    <div className="stat-card">
            <span className="stat-label">Tamamlanan</span>
            <span className="stat-value">{myProgram.completedCourses}</span>
     </div>
   <div className="stat-card">
       <span className="stat-label">Toplam Kredi</span>
        <span className="stat-value">{myProgram.totalCredits}</span>
  </div>
          <div className="stat-card">
          <span className="stat-label">GPA</span>
     <span className="stat-value">
          {myProgram.gpa ? myProgram.gpa.toFixed(2) : 'N/A'}
            </span>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="filters">
     <select
          value={selectedCategory || ''}
          onChange={(e) => setSelectedCategory(e.target.value || null)}
        >
          <option value="">Tüm Kategoriler</option>
          {categories.map(cat => (
    <option key={cat.id} value={cat.id}>
       {cat.name} ({cat.courseCount})
       </option>
       ))}
        </select>

     <select
          value={selectedSemester || ''}
          onChange={(e) => setSelectedSemester(e.target.value || null)}
      >
  <option value="">Tüm Dönemler</option>
          {[1, 2, 3, 4, 5, 6, 7, 8].map(sem => (
   <option key={sem} value={sem}>
            {sem}. Dönem
   </option>
          ))}
        </select>

        <label>
          <input
            type="checkbox"
  checked={showElectivesOnly}
      onChange={(e) => setShowElectivesOnly(e.target.checked)}
       />
    Sadece Seçmeli Dersler
  </label>

      <input
       type="text"
   placeholder="Ders kodu veya adý ara..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>

      {/* Courses List */}
      {loading ? (
<div className="loading">Yükleniyor...</div>
 ) : (
     <div className="courses-grid">
        {courses.map(course => (
            <div
        key={course.id}
              className={`course-card ${getCourseStatus(course.id)}`}
        >
        <div className="course-header">
      <h3>{course.courseCode}</h3>
   {course.isElective && <span className="badge elective">Seçmeli</span>}
       {isEnrolled(course.id) && (
            <span className={`badge ${getCourseStatus(course.id)}`}>
          {getCourseStatus(course.id) === 'completed' ? '? Tamamlandý' : '?? Kayýtlý'}
       </span>
     )}
           </div>

       <h4>{course.courseName}</h4>
     
              <p className="course-description">
            {course.description || 'Açýklama bulunmuyor'}
          </p>

    <div className="course-details">
       <div className="detail">
<span className="label">Dönem:</span>
               <span className="value">{course.semester || 'Belirsiz'}</span>
                </div>
 <div className="detail">
         <span className="label">Teori/Uygulama:</span>
       <span className="value">
        {course.theoryHours}+{course.practiceHours}
     </span>
          </div>
                <div className="detail">
      <span className="label">Kredi:</span>
    <span className="value">{course.credits}</span>
 </div>
 <div className="detail">
        <span className="label">AKTS:</span>
         <span className="value">{course.ects}</span>
             </div>
   <div className="detail">
         <span className="label">Kategori:</span>
   <span className="value">{course.category.name}</span>
                </div>
 </div>

       <button
    className="btn-details"
     onClick={() => viewCourseDetails(course.id)}
 >
      Detaylarý Gör
              </button>
      </div>
          ))}
        </div>
      )}

      {courses.length === 0 && !loading && (
     <div className="no-results">
          <p>Ders bulunamadý. Filtreleri deðiþtirmeyi deneyin.</p>
        </div>
      )}
    </div>
  );
};

export default CoursesPage;
```

---

## ?? CSS Styling

```css
/* courses.css */
.courses-page {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 30px;
}

.page-header h1 {
  font-size: 2em;
  margin-bottom: 10px;
}

.student-stats {
display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 30px;
}

.stat-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 20px;
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.stat-label {
  font-size: 0.9em;
  opacity: 0.9;
  margin-bottom: 5px;
}

.stat-value {
  font-size: 2em;
  font-weight: bold;
}

.filters {
  display: flex;
  gap: 15px;
  margin-bottom: 30px;
  flex-wrap: wrap;
  align-items: center;
}

.filters select,
.filters input[type="text"] {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 5px;
  font-size: 1em;
}

.filters label {
  display: flex;
  align-items: center;
  gap: 5px;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.course-card {
  background: white;
  border: 2px solid #e0e0e0;
  border-radius: 10px;
  padding: 20px;
  transition: all 0.3s ease;
}

.course-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.3);
  border-color: #667eea;
}

.course-card.enrolled {
  border-color: #ffa726;
  background: #fff8e1;
}

.course-card.completed {
  border-color: #66bb6a;
  background: #e8f5e9;
}

.course-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.course-header h3 {
  color: #667eea;
  margin: 0;
}

.badge {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.75em;
  font-weight: 600;
}

.badge.elective {
  background: #e3f2fd;
  color: #1976d2;
}

.badge.enrolled {
  background: #fff8e1;
  color: #f57c00;
}

.badge.completed {
  background: #e8f5e9;
  color: #2e7d32;
}

.course-card h4 {
  margin: 10px 0;
  font-size: 1.1em;
  color: #333;
}

.course-description {
  color: #666;
  font-size: 0.9em;
  margin-bottom: 15px;
  line-height: 1.5;
  min-height: 3em;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.course-details {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 10px;
  margin-bottom: 15px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 5px;
}

.detail {
  display: flex;
  justify-content: space-between;
}

.detail .label {
  font-weight: 600;
  color: #666;
  font-size: 0.85em;
}

.detail .value {
  color: #333;
  font-size: 0.85em;
}

.btn-details {
  width: 100%;
  padding: 10px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 5px;
  font-size: 1em;
  cursor: pointer;
  transition: background 0.3s ease;
}

.btn-details:hover {
  background: #764ba2;
}

.loading,
.no-results {
  text-align: center;
  padding: 40px;
  color: #666;
}

@media (max-width: 768px) {
  .courses-grid {
    grid-template-columns: 1fr;
  }
  
  .filters {
    flex-direction: column;
    align-items: stretch;
  }
  
  .filters select,
  .filters input {
    width: 100%;
  }
}
```

---

## ?? Authorization

All course endpoints require authentication:

| Endpoint | Roles |
|----------|-------|
| `GET /api/courses` | All (Student, Advisor, Admin) |
| `GET /api/courses/{id}` | All |
| `GET /api/courses/categories` | All |
| `GET /api/courses/by-semester/{semester}` | All |
| `GET /api/courses/electives` | All |
| `POST /api/courses` | **Admin Only** |
| `PUT /api/courses/{id}` | **Admin Only** |
| `DELETE /api/courses/{id}` | **Admin Only** |
| `GET /api/student-courses/my-program` | **Student** |

---

## ? Testing

### 1. Test All Courses Endpoint

```bash
curl -X GET "https://localhost:44375/api/courses" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected:** List of all courses with descriptions

### 2. Test Categories

```bash
curl -X GET "https://localhost:44375/api/courses/categories" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected:** List of categories with course counts

### 3. Test Student Program

```bash
curl -X GET "https://localhost:44375/api/student-courses/my-program" \
  -H "Authorization: Bearer STUDENT_TOKEN"
```

**Expected:** Student's enrolled courses with grades and status

---

## ?? Summary

**Fixed Endpoints:**
- ? All courses: `/api/courses`
- ? Course details: `/api/courses/{id}`
- ? Categories: `/api/courses/categories`
- ? Student program: `/api/student-courses/my-program`

**Features:**
- ? Filter by category, semester, elective status
- ? Search by course code or name
- ? Student enrollment status display
- ? GPA and credit statistics (students)
- ? Course descriptions visible
- ? Responsive design

**Next Steps:**
1. Update frontend `Courses.jsx` with new code
2. Add `courses.css` styling
3. Test with different user roles
4. Verify filtering works

---

**Status:** ? Ready for Implementation  
**Date:** 2025-01-07  
**Backend:** ? Working  
**Frontend:** ?? Needs Update
