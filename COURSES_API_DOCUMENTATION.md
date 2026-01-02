# ?? Courses API - Complete Documentation

**Version:** 1.0  
**Date:** 2025-01-07  
**Authorization:** All roles (Student, Advisor, Admin)

---

## ?? Overview

The Courses API provides endpoints for managing and viewing course information, categories, and student enrollments. All endpoints require authentication.

**Base URL:** `https://localhost:7175/api/courses`

**Authorization:**
- ? **View courses:** All authenticated users (Student, Advisor, Admin)
- ? **Create/Edit/Delete:** Admin only

---

## ?? Endpoints

### 1. Get All Courses

```http
GET /api/courses
Authorization: Bearer {token}
```

**Query Parameters (All Optional):**
| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `categoryId` | int | Filter by category | `categoryId=1` |
| `semester` | int | Filter by semester (1-8) | `semester=3` |
| `isElective` | bool | Filter elective/required | `isElective=false` |
| `search` | string | Search in code/name | `search=matematik` |

**Example Requests:**
```http
# All courses
GET /api/courses

# 3rd semester courses
GET /api/courses?semester=3

# Required courses only
GET /api/courses?isElective=false

# Search for "matematik"
GET /api/courses?search=matematik

# Category 1 + Semester 3 + Required
GET /api/courses?categoryId=1&semester=3&isElective=false
```

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
      "description": "Bilgisayar biliminin temel kavramlarý ve programlama temelleri. Bu ders öðrencilere bilgisayar bilimleri alanýnda temel bilgileri kazandýrmayý amaçlar.",
      "category": {
        "id": 1,
        "name": "Temel Bilgisayar Bilimleri",
        "displayOrder": 1
      }
    },
    {
   "id": 2,
      "courseCode": "MAT101",
      "courseName": "Matematik I",
      "theoryHours": 4,
      "practiceHours": 0,
   "credits": 4,
      "ects": 6,
      "semester": 1,
      "isElective": false,
      "description": "Temel matematik konularý: Limit, türev, integral ve uygulamalarý.",
      "category": {
        "id": 2,
        "name": "Matematik ve Ýstatistik",
        "displayOrder": 2
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

**Example:**
```http
GET /api/courses/1
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
  "description": "Bilgisayar biliminin temel kavramlarý ve programlama temelleri. Bu ders öðrencilere bilgisayar bilimleri alanýnda temel bilgileri kazandýrmayý amaçlar. Algoritma tasarýmý, veri yapýlarý ve yazýlým geliþtirme süreçleri konularýný kapsar.",
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

**Note:** Includes prerequisite courses (if any)

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
    "name": "Matematik ve Ýstatistik",
    "description": "Matematik ve istatistik dersleri",
    "displayOrder": 2,
    "courseCount": 8
  },
  {
    "id": 3,
    "name": "Yazýlým Geliþtirme",
    "description": "Yazýlým geliþtirme ve mühendislik dersleri",
    "displayOrder": 3,
    "courseCount": 15
  },
  {
    "id": 4,
    "name": "Veri Bilimi",
 "description": "Veri bilimi ve yapay zeka dersleri",
    "displayOrder": 4,
  "courseCount": 10
  }
]
```

---

### 4. Get Courses by Semester

```http
GET /api/courses/by-semester/{semester}
Authorization: Bearer {token}
```

**Example:**
```http
GET /api/courses/by-semester/3
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
    },
    {
      "id": 16,
      "courseCode": "BÝL302",
      "courseName": "Veritabaný Sistemleri",
"theoryHours": 3,
      "practiceHours": 2,
      "credits": 4,
      "ects": 6,
  "isElective": false,
      "category": "Veri Bilimi"
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
      "category": "Veri Bilimi"
    },
    {
      "id": 46,
      "courseCode": "BÝL402",
      "courseName": "Mobil Uygulama Geliþtirme",
      "theoryHours": 2,
    "practiceHours": 2,
      "credits": 3,
      "ects": 5,
      "category": "Yazýlým Geliþtirme"
    }
  ]
}
```

---

## ?? Student Course Program

### Get My Enrolled Courses

```http
GET /api/student-courses/my-program
Authorization: Bearer {token}
```

**Authorization:** Student role required

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
    },
    {
      "id": 2,
      "semester": 1,
      "courseId": 2,
      "courseCode": "MAT101",
      "courseName": "Matematik I",
      "theoryHours": 4,
 "practiceHours": 0,
 "credits": 4,
      "ects": 6,
      "isElective": false,
      "category": "Matematik ve Ýstatistik",
      "isCompleted": true,
      "grade": 78.0,
  "letterGrade": "BB",
   "completionDate": "2024-06-15T00:00:00Z",
      "enrolledAt": "2024-02-01T00:00:00Z"
    }
  ]
}
```

---

## ?? Admin Endpoints

### Create Course

```http
POST /api/courses
Authorization: Bearer {token}
Content-Type: application/json
```

**Authorization:** Admin only

**Request Body:**
```json
{
  "courseCode": "BÝL501",
  "courseName": "Ýleri Algoritma Tasarýmý",
  "theoryHours": 3,
  "practiceHours": 0,
  "credits": 3,
  "ects": 5,
  "categoryId": 1,
  "semester": 5,
  "isElective": true,
  "description": "Ýleri algoritma tasarýmý teknikleri ve karmaþýklýk analizi"
}
```

**Response:**
```json
{
  "message": "Course created successfully",
  "courseId": 50
}
```

**Error (Duplicate Code):**
```json
{
  "error": "Course code already exists"
}
```

---

### Update Course

```http
PUT /api/courses/{id}
Authorization: Bearer {token}
Content-Type: application/json
```

**Authorization:** Admin only

**Request Body:**
```json
{
  "courseCode": "BÝL501",
  "courseName": "Ýleri Algoritma Tasarýmý",
  "theoryHours": 3,
  "practiceHours": 0,
  "credits": 3,
  "ects": 5,
  "categoryId": 1,
  "semester": 5,
  "isElective": true,
  "description": "Güncellenmiþ açýklama"
}
```

**Response:**
```json
{
  "message": "Course updated successfully"
}
```

---

### Delete Course

```http
DELETE /api/courses/{id}
Authorization: Bearer {token}
```

**Authorization:** Admin only

**Response:**
```json
{
  "message": "Course deleted successfully"
}
```

---

## ?? Frontend Examples

### React: Load All Courses with Filters

```javascript
import { useState, useEffect } from 'react';
import api from '../services/api';

const CoursesPage = () => {
  const [courses, setCourses] = useState([]);
  const [categories, setCategories] = useState([]);
  const [filters, setFilters] = useState({
    categoryId: null,
 semester: null,
    isElective: null,
    search: ''
  });

useEffect(() => {
    loadCourses();
    loadCategories();
  }, [filters]);

  const loadCourses = async () => {
    try {
      const params = new URLSearchParams();
      if (filters.categoryId) params.append('categoryId', filters.categoryId);
      if (filters.semester) params.append('semester', filters.semester);
   if (filters.isElective !== null) params.append('isElective', filters.isElective);
      if (filters.search) params.append('search', filters.search);

      const response = await api.get(`/courses?${params.toString()}`);
      setCourses(response.data.courses);
    } catch (error) {
      console.error('Failed to load courses:', error);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await api.get('/courses/categories');
      setCategories(response.data);
    } catch (error) {
      console.error('Failed to load categories:', error);
 }
  };

  return (
    <div>
      <h1>Dersler</h1>
      
      {/* Filters */}
      <div className="filters">
        <select
          value={filters.categoryId || ''}
          onChange={(e) => setFilters({...filters, categoryId: e.target.value || null})}
        >
        <option value="">Tüm Kategoriler</option>
     {categories.map(cat => (
            <option key={cat.id} value={cat.id}>
     {cat.name} ({cat.courseCount})
   </option>
          ))}
        </select>

        <select
      value={filters.semester || ''}
          onChange={(e) => setFilters({...filters, semester: e.target.value || null})}
        >
    <option value="">Tüm Dönemler</option>
          {[1,2,3,4,5,6,7,8].map(sem => (
            <option key={sem} value={sem}>{sem}. Dönem</option>
          ))}
   </select>

        <input
       type="text"
          placeholder="Ders ara..."
       value={filters.search}
          onChange={(e) => setFilters({...filters, search: e.target.value})}
 />
    </div>

      {/* Courses Grid */}
      <div className="courses-grid">
        {courses.map(course => (
          <div key={course.id} className="course-card">
            <h3>{course.courseCode}</h3>
     <h4>{course.courseName}</h4>
            <p>{course.description}</p>
      <div className="course-info">
       <span>Kredi: {course.credits}</span>
              <span>AKTS: {course.ects}</span>
     <span>Dönem: {course.semester}</span>
      </div>
            {course.isElective && <span className="badge">Seçmeli</span>}
     </div>
))}
      </div>
    </div>
  );
};
```

---

### Vue.js: Load Student Program

```javascript
<template>
  <div class="my-program">
    <h1>Ders Programým</h1>
  
<div class="stats">
      <div class="stat">
        <span class="label">Toplam Ders</span>
     <span class="value">{{ program?.totalCourses }}</span>
      </div>
      <div class="stat">
    <span class="label">Tamamlanan</span>
        <span class="value">{{ program?.completedCourses }}</span>
      </div>
      <div class="stat">
        <span class="label">Toplam Kredi</span>
        <span class="value">{{ program?.totalCredits }}</span>
      </div>
   <div class="stat">
        <span class="label">GPA</span>
        <span class="value">{{ program?.gpa?.toFixed(2) }}</span>
      </div>
    </div>

    <div v-for="course in program?.courses" :key="course.id" class="course">
      <h3>{{ course.courseCode }} - {{ course.courseName }}</h3>
      <div class="course-details">
   <span>Dönem: {{ course.semester }}</span>
        <span>Kredi: {{ course.credits }}</span>
        <span v-if="course.isCompleted">
          Not: {{ course.grade }} ({{ course.letterGrade }})
        </span>
    <span v-else class="pending">Devam Ediyor</span>
      </div>
    </div>
  </div>
</template>

<script>
import api from '@/services/api';

export default {
  data() {
    return {
      program: null
    };
  },
  async mounted() {
    await this.loadProgram();
  },
  methods: {
    async loadProgram() {
      try {
        const response = await api.get('/student-courses/my-program');
    this.program = response.data;
      } catch (error) {
        console.error('Failed to load program:', error);
      }
    }
  }
};
</script>
```

---

## ?? UI Components

### Course Card

```html
<div class="course-card">
  <div class="course-header">
    <h3>BÝL101</h3>
    <span class="badge required">Zorunlu</span>
  </div>
  
  <h4>Bilgisayar Bilimine Giriþ</h4>
  
  <p class="description">
  Bilgisayar biliminin temel kavramlarý ve programlama temelleri.
  </p>
  
  <div class="course-info">
    <div class="info-item">
      <span class="label">Dönem</span>
      <span class="value">1</span>
    </div>
<div class="info-item">
      <span class="label">Teori/Uygulama</span>
      <span class="value">3+2</span>
    </div>
<div class="info-item">
      <span class="label">Kredi</span>
  <span class="value">4</span>
    </div>
    <div class="info-item">
      <span class="label">AKTS</span>
      <span class="value">6</span>
    </div>
  </div>
  
  <button class="btn-details">Detaylarý Gör</button>
</div>
```

**CSS:**
```css
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
}

.course-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.badge {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.75em;
  font-weight: 600;
}

.badge.required {
  background: #e3f2fd;
  color: #1976d2;
}

.badge.elective {
  background: #fff3e0;
  color: #f57c00;
}

.description {
  color: #666;
  font-size: 0.9em;
  margin: 10px 0;
line-height: 1.5;
}

.course-info {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 10px;
  margin: 15px 0;
  padding: 15px;
  background: #f8f9fa;
border-radius: 5px;
}

.info-item {
  display: flex;
  justify-content: space-between;
}

.info-item .label {
  font-weight: 600;
  color: #666;
}

.btn-details {
  width: 100%;
  padding: 10px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}
```

---

## ? Summary

**Available Endpoints:**
- ? `GET /api/courses` - All courses (with filters)
- ? `GET /api/courses/{id}` - Course details
- ? `GET /api/courses/categories` - Categories
- ? `GET /api/courses/by-semester/{semester}` - Semester courses
- ? `GET /api/courses/electives` - Elective courses
- ? `GET /api/student-courses/my-program` - Student's enrolled courses
- ? `POST /api/courses` - Create (Admin)
- ? `PUT /api/courses/{id}` - Update (Admin)
- ? `DELETE /api/courses/{id}` - Delete (Admin)

**Authorization:**
- ??? **View:** All roles
- ?? **Create/Edit/Delete:** Admin only
- ?? **My Program:** Students only

**Key Features:**
- Filtering by category, semester, elective status
- Search by course code or name
- Prerequisites support
- Student program with GPA tracking
- Course categories

---

**Status:** ? Ready  
**Date:** 2025-01-07  
**Backend:** ? Working  
**Frontend:** ?? Update Required (See COURSES_FRONTEND_FIX.md)
