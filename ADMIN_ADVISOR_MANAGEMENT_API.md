# ????? Admin Advisor Management API (v3.0)

**Version:** 3.0.0  
**Date:** December 20, 2024  
**Purpose:** Simplified admin-focused advisor assignment system

---

## ?? Overview

Bu API, **sadece adminler** için tasarlanmýþ basitleþtirilmiþ bir öðretmen atama sistemidir.

### Key Features

- ? **Admin-only access** - Tüm endpoint'ler sadece Admin rolü için
- ? **Complete student list** - Tüm öðrenciler advisor bilgisiyle birlikte
- ? **Easy assignment** - Tek endpoint ile öðretmen atama/güncelleme
- ? **Automatic notifications** - Hem öðrenci hem öðretmene bildirim
- ? **No document dependency** - Belge olmadan öðretmen atamasý

---

## ?? API Endpoints

### 1. Get All Advisors

```http
GET /api/advisors
Authorization: Bearer {admin-token}
```

**Authorization:** Admin only

**Response:**
```json
{
  "totalAdvisors": 5,
  "advisors": [
    {
      "id": "advisor-id-1",
      "userName": "prof.smith@university.edu",
      "email": "prof.smith@university.edu",
      "emailConfirmed": true
    },
    {
      "id": "advisor-id-2",
      "userName": "prof.johnson@university.edu",
    "email": "prof.johnson@university.edu",
      "emailConfirmed": true
  }
  ]
}
```

---

### 2. Assign Advisor to Student

```http
POST /api/advisors/assign
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "studentId": "student-user-id-123",
  "advisorId": "advisor-user-id-456"
}
```

**Authorization:** Admin only

**Request Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `studentId` | string | ? | Student's user ID |
| `advisorId` | string | ? | Advisor's user ID |

**Validation:**
- ? Student exists and has "Student" role
- ? Advisor exists and has "Advisor" role
- ? Handles both new assignment and update

**Response (New Assignment):**
```json
{
  "message": "Öðretmen baþarýyla atandý",
  "studentId": "student-id-123",
  "studentName": "student@university.edu",
  "advisorId": "advisor-id-456",
  "advisorName": "prof.smith@university.edu",
  "isUpdate": false
}
```

**Response (Update):**
```json
{
  "message": "Öðretmen baþarýyla güncellendi",
  "studentId": "student-id-123",
  "studentName": "student@university.edu",
  "advisorId": "advisor-id-789",
  "advisorName": "prof.johnson@university.edu",
  "isUpdate": true
}
```

**Side Effects:**

**New Assignment:**
1. ? Student receives notification: "Öðretmen Atandý"
2. ? New advisor receives notification: "Yeni Öðrenci Atandý"

**Update (Changing Advisor):**
1. ? Student receives notification: "Öðretmeniniz Deðiþtirildi"
2. ? New advisor receives notification: "Yeni Öðrenci Atandý"
3. ? Previous advisor receives notification: "Öðrenci Atamasý Kaldýrýldý"

**Error Responses:**

404 Not Found (Student):
```json
{
  "error": "Student not found"
}
```

400 Bad Request (Not a Student):
```json
{
  "error": "User is not a student"
}
```

404 Not Found (Advisor):
```json
{
  "error": "Advisor not found"
}
```

400 Bad Request (Not an Advisor):
```json
{
  "error": "User is not an advisor"
}
```

---

### 3. Remove Advisor from Student

```http
DELETE /api/advisors/remove/{studentId}
Authorization: Bearer {admin-token}
```

**Authorization:** Admin only

**Path Parameters:**
- `studentId`: Student's user ID

**Response:**
```json
{
  "message": "Öðretmen atamasý baþarýyla kaldýrýldý",
  "studentId": "student-id-123",
  "studentName": "student@university.edu"
}
```

**Side Effects:**
1. ? Student receives notification: "Öðretmen Atamasý Kaldýrýldý"
2. ? Previous advisor receives notification: "Öðrenci Atamasý Kaldýrýldý"

**Error Responses:**

404 Not Found:
```json
{
  "error": "Student not found"
}
```

400 Bad Request (No Advisor):
```json
{
  "error": "Student does not have an advisor"
}
```

---

### 4. Get Advisor Details

```http
GET /api/advisors/{advisorId}
Authorization: Bearer {admin-token}
```

**Authorization:** Admin only

**Response:**
```json
{
  "id": "advisor-id-456",
  "userName": "prof.smith@university.edu",
  "email": "prof.smith@university.edu",
  "emailConfirmed": true,
  "assignedStudentsCount": 12,
  "students": [
    {
      "id": "student-id-1",
      "userName": "student1@university.edu",
    "email": "student1@university.edu",
      "emailConfirmed": true
    },
    {
      "id": "student-id-2",
      "userName": "student2@university.edu",
      "email": "student2@university.edu",
      "emailConfirmed": false
    }
  ]
}
```

---

### 5. Get All Students (Updated)

```http
GET /api/students?search=john&page=1&pageSize=20
Authorization: Bearer {admin-token}
```

**Authorization:** Admin or Advisor

**Query Parameters:**
- `search` (optional): Search by email or username
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page

**Response (WITH ADVISOR INFO):**
```json
{
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "students": [
    {
      "id": "student-id-123",
      "userName": "john.doe@university.edu",
      "email": "john.doe@university.edu",
      "emailConfirmed": true,
      "documentCount": 5,
      "pendingSubmissions": 2,
      "hasAdvisor": true,
      "advisor": {
     "id": "advisor-id-456",
        "userName": "prof.smith@university.edu",
        "email": "prof.smith@university.edu"
      }
    },
    {
 "id": "student-id-456",
      "userName": "jane.doe@university.edu",
 "email": "jane.doe@university.edu",
      "emailConfirmed": false,
      "documentCount": 0,
      "pendingSubmissions": 0,
      "hasAdvisor": false,
      "advisor": null
    }
  ]
}
```

---

### 6. Get Students Without Advisor (Updated)

```http
GET /api/students/without-advisor
Authorization: Bearer {admin-token}
```

**Authorization:** Admin or Advisor

**Response:**
```json
{
  "totalCount": 8,
  "students": [
    {
 "id": "student-id-789",
      "userName": "student.no.advisor@university.edu",
   "email": "student.no.advisor@university.edu",
    "emailConfirmed": true,
      "documentCount": 2
    }
  ]
}
```

---

## ?? Admin UI Integration

### Complete Admin Panel Example

```html
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <title>Öðretmen Atama Paneli</title>
    <style>
   * { margin: 0; padding: 0; box-sizing: border-box; }
     body { font-family: Arial, sans-serif; padding: 20px; background: #f5f5f5; }
        .container { max-width: 1400px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; }
        h1 { color: #333; margin-bottom: 20px; }
   
        .search-section {
      display: flex;
    gap: 10px;
  margin-bottom: 20px;
    }
        
        .search-input {
            flex: 1;
            padding: 10px;
   border: 1px solid #ddd;
            border-radius: 4px;
      font-size: 14px;
   }
        
        .filter-buttons {
    display: flex;
            gap: 10px;
        }
      
   .btn {
  padding: 10px 20px;
            border: none;
       border-radius: 4px;
            cursor: pointer;
   font-size: 14px;
        }
        
        .btn-primary { background: #007bff; color: white; }
        .btn-primary:hover { background: #0056b3; }
    .btn-secondary { background: #6c757d; color: white; }
        .btn-success { background: #28a745; color: white; }
        .btn-danger { background: #dc3545; color: white; }
        .btn-sm { padding: 6px 12px; font-size: 12px; }
        
        table {
         width: 100%;
         border-collapse: collapse;
      margin-top: 20px;
        }
        
        th, td {
 padding: 12px;
         text-align: left;
     border-bottom: 1px solid #ddd;
        }
        
     th {
            background: #f8f9fa;
     font-weight: 600;
        }
        
tr:hover {
 background: #f8f9fa;
        }
        
        .status-badge {
    padding: 4px 8px;
         border-radius: 4px;
       font-size: 12px;
      font-weight: 600;
        }
    
        .status-assigned { background: #d4edda; color: #155724; }
        .status-unassigned { background: #fff3cd; color: #856404; }
     
        .modal {
 display: none;
          position: fixed;
        top: 0;
            left: 0;
          width: 100%;
        height: 100%;
     background: rgba(0,0,0,0.5);
          z-index: 1000;
        }
        
   .modal.active {
          display: flex;
            align-items: center;
          justify-content: center;
        }
        
        .modal-content {
       background: white;
            padding: 30px;
            border-radius: 8px;
   max-width: 500px;
  width: 90%;
        }
        
        .modal-header {
      margin-bottom: 20px;
   }
        
   .modal-header h2 {
       margin-bottom: 5px;
        }
        
        .form-group {
    margin-bottom: 15px;
        }
        
  .form-group label {
display: block;
       margin-bottom: 5px;
        font-weight: 600;
        }
        
        .form-control {
      width: 100%;
    padding: 10px;
 border: 1px solid #ddd;
    border-radius: 4px;
        }
  
   .modal-footer {
   display: flex;
          gap: 10px;
        justify-content: flex-end;
            margin-top: 20px;
        }
        
        .stats {
 display: grid;
   grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
         margin-bottom: 20px;
     }
        
        .stat-card {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
        padding: 20px;
            border-radius: 8px;
        }
  
        .stat-label {
  font-size: 14px;
            opacity: 0.9;
        }
 
   .stat-value {
      font-size: 32px;
          font-weight: bold;
        margin-top: 5px;
      }
    </style>
</head>
<body>
    <div class="container">
        <h1>????? Öðretmen Atama Yönetimi</h1>
      
   <!-- Statistics -->
   <div class="stats">
            <div class="stat-card">
       <div class="stat-label">Toplam Öðrenci</div>
    <div class="stat-value" id="totalStudents">-</div>
    </div>
            <div class="stat-card">
 <div class="stat-label">Öðretmeni Olan</div>
        <div class="stat-value" id="assignedStudents">-</div>
        </div>
  <div class="stat-card">
             <div class="stat-label">Öðretmeni Olmayan</div>
       <div class="stat-value" id="unassignedStudents">-</div>
 </div>
         <div class="stat-card">
        <div class="stat-label">Toplam Öðretmen</div>
     <div class="stat-value" id="totalAdvisors">-</div>
            </div>
        </div>
        
     <!-- Search and Filters -->
        <div class="search-section">
            <input 
    type="text" 
         id="searchInput" 
                class="search-input" 
      placeholder="Öðrenci ara (email, isim)..."
     oninput="searchStudents()"
      >
            <div class="filter-buttons">
         <button class="btn btn-primary" onclick="loadAllStudents()">
      Tümü
   </button>
 <button class="btn btn-secondary" onclick="loadUnassignedStudents()">
        Öðretmensizler
       </button>
              <button class="btn btn-success" onclick="refreshData()">
    Yenile
    </button>
   </div>
        </div>
     
        <!-- Students Table -->
        <table id="studentsTable">
            <thead>
          <tr>
                    <th>Öðrenci</th>
            <th>Email</th>
   <th>Belge Sayýsý</th>
          <th>Durum</th>
       <th>Öðretmen</th>
        <th>Ýþlemler</th>
             </tr>
     </thead>
  <tbody id="studentsTableBody">
    <tr>
    <td colspan="6" style="text-align: center; padding: 40px;">
Yükleniyor...
       </td>
      </tr>
 </tbody>
     </table>
    </div>
    
    <!-- Advisor Selection Modal -->
    <div id="advisorModal" class="modal">
        <div class="modal-content">
 <div class="modal-header">
       <h2>Öðretmen Seç</h2>
         <p id="modalStudentName" style="color: #666;"></p>
       </div>
    <div class="form-group">
             <label for="advisorSelect">Öðretmen:</label>
      <select id="advisorSelect" class="form-control">
     <option value="">Seçiniz...</option>
      </select>
 </div>
  <div class="modal-footer">
  <button class="btn btn-secondary" onclick="closeModal()">
             Ýptal
                </button>
       <button class="btn btn-primary" onclick="confirmAssignment()">
         Ata
         </button>
        </div>
        </div>
    </div>

    <script>
    const API_URL = 'https://localhost:7175/api';
        const token = localStorage.getItem('token');

        let allStudents = [];
 let allAdvisors = [];
        let selectedStudent = null;
     
        // Initialize
        document.addEventListener('DOMContentLoaded', () => {
      if (!token) {
   alert('Lütfen giriþ yapýn');
         window.location.href = '/login.html';
            return;
 }
     
            loadInitialData();
   });
        
        async function loadInitialData() {
         try {
             await Promise.all([
    loadAdvisors(),
            loadAllStudents()
  ]);
    updateStats();
        } catch (error) {
                console.error('Error loading initial data:', error);
           alert('Veri yüklenirken hata oluþtu');
            }
  }
        
    async function loadAdvisors() {
   const response = await fetch(`${API_URL}/advisors`, {
     headers: { 'Authorization': `Bearer ${token}` }
            });
         
      if (!response.ok) throw new Error('Failed to load advisors');
            
    const data = await response.json();
  allAdvisors = data.advisors;
          
    // Populate advisor select
 const select = document.getElementById('advisorSelect');
            select.innerHTML = '<option value="">Seçiniz...</option>';
         allAdvisors.forEach(advisor => {
       const option = document.createElement('option');
        option.value = advisor.id;
    option.textContent = `${advisor.userName} (${advisor.email})`;
        select.appendChild(option);
            });
        }
        
    async function loadAllStudents() {
   const response = await fetch(`${API_URL}/students?pageSize=1000`, {
            headers: { 'Authorization': `Bearer ${token}` }
     });
        
if (!response.ok) throw new Error('Failed to load students');
    
            const data = await response.json();
        allStudents = data.students;
            renderStudents(allStudents);
        }
        
        async function loadUnassignedStudents() {
            const response = await fetch(`${API_URL}/students/without-advisor`, {
          headers: { 'Authorization': `Bearer ${token}` }
 });
        
            if (!response.ok) throw new Error('Failed to load unassigned students');
    
   const data = await response.json();
     renderStudents(data.students.map(s => ({
          ...s,
         hasAdvisor: false,
          advisor: null,
                pendingSubmissions: 0
 })));
        }
   
        function searchStudents() {
   const query = document.getElementById('searchInput').value.toLowerCase();
         if (!query) {
           renderStudents(allStudents);
       return;
            }
            
   const filtered = allStudents.filter(s => 
       s.userName.toLowerCase().includes(query) ||
      s.email.toLowerCase().includes(query) ||
     (s.advisor && s.advisor.userName.toLowerCase().includes(query))
    );
     renderStudents(filtered);
        }
     
        function renderStudents(students) {
            const tbody = document.getElementById('studentsTableBody');
            
            if (students.length === 0) {
      tbody.innerHTML = `
           <tr>
     <td colspan="6" style="text-align: center; padding: 40px; color: #999;">
    Öðrenci bulunamadý
   </td>
      </tr>
  `;
   return;
     }
      
            tbody.innerHTML = students.map(student => `
      <tr>
    <td>${student.userName}</td>
          <td>${student.email}</td>
     <td>${student.documentCount || 0}</td>
           <td>
         <span class="status-badge ${student.hasAdvisor ? 'status-assigned' : 'status-unassigned'}">
  ${student.hasAdvisor ? '? Atandý' : '?? Atanmadý'}
   </span>
         </td>
          <td>
          ${student.hasAdvisor ? 
`${student.advisor.userName}` : 
      '<em style="color: #999;">Atanmamýþ</em>'
        }
        </td>
      <td>
                <button 
       class="btn btn-primary btn-sm" 
   onclick='openAssignModal(${JSON.stringify(student)})'>
       ${student.hasAdvisor ? 'Deðiþtir' : 'Ata'}
           </button>
  ${student.hasAdvisor ? 
      `<button 
        class="btn btn-danger btn-sm" 
          onclick="removeAdvisor('${student.id}', '${student.userName}')">
            Kaldýr
      </button>` 
      : ''}
     </td>
 </tr>
            `).join('');
        }
   
        function openAssignModal(student) {
 selectedStudent = student;
       document.getElementById('modalStudentName').textContent = 
       `Öðrenci: ${student.userName}`;
      
        if (student.hasAdvisor) {
      document.getElementById('advisorSelect').value = student.advisor.id;
      } else {
       document.getElementById('advisorSelect').value = '';
            }

        document.getElementById('advisorModal').classList.add('active');
        }
        
        function closeModal() {
         document.getElementById('advisorModal').classList.remove('active');
            selectedStudent = null;
        }
        
   async function confirmAssignment() {
    const advisorId = document.getElementById('advisorSelect').value;
     if (!advisorId) {
     alert('Lütfen bir öðretmen seçin');
     return;
    }
       
    try {
  const response = await fetch(`${API_URL}/advisors/assign`, {
               method: 'POST',
       headers: {
       'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
   },
          body: JSON.stringify({
 studentId: selectedStudent.id,
    advisorId: advisorId
        })
      });
     
   const data = await response.json();
  
       if (response.ok) {
         alert(`? ${data.message}`);
                  closeModal();
           await loadAllStudents();
         updateStats();
                } else {
         alert(`? Hata: ${data.error}`);
            }
   } catch (error) {
          console.error('Error assigning advisor:', error);
     alert('Atama sýrasýnda hata oluþtu');
     }
        }
        
   async function removeAdvisor(studentId, studentName) {
            if (!confirm(`${studentName} öðrencisinin öðretmen atamasýný kaldýrmak istediðinizden emin misiniz?`)) {
      return;
            }
  
            try {
      const response = await fetch(`${API_URL}/advisors/remove/${studentId}`, {
        method: 'DELETE',
           headers: { 'Authorization': `Bearer ${token}` }
          });
  
                const data = await response.json();
      
                if (response.ok) {
      alert(`? ${data.message}`);
           await loadAllStudents();
   updateStats();
      } else {
              alert(`? Hata: ${data.error}`);
        }
 } catch (error) {
   console.error('Error removing advisor:', error);
  alert('Kaldýrma sýrasýnda hata oluþtu');
            }
        }
     
        function updateStats() {
     const assigned = allStudents.filter(s => s.hasAdvisor).length;
     const unassigned = allStudents.filter(s => !s.hasAdvisor).length;
  
            document.getElementById('totalStudents').textContent = allStudents.length;
       document.getElementById('assignedStudents').textContent = assigned;
     document.getElementById('unassignedStudents').textContent = unassigned;
            document.getElementById('totalAdvisors').textContent = allAdvisors.length;
        }
 
        async function refreshData() {
      await loadInitialData();
            alert('Veriler güncellendi');
   }
    </script>
</body>
</html>
```

---

## ?? Authorization

**All endpoints require:**
- ? Valid JWT token
- ? Admin role

**Access denied for:**
- ? Unauthenticated users ? 401 Unauthorized
- ? Non-admin users ? 403 Forbidden

---

## ?? Testing

### Postman Collection

```json
{
  "info": {
    "name": "Advisor Management v3.0",
 "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
    "name": "Get All Advisors",
      "request": {
"method": "GET",
        "header": [
{
    "key": "Authorization",
      "value": "Bearer {{token}}"
    }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/advisors",
          "host": ["{{baseUrl}}"],
          "path": ["api", "advisors"]
     }
      }
    },
    {
      "name": "Assign Advisor",
 "request": {
        "method": "POST",
    "header": [
          {
 "key": "Authorization",
     "value": "Bearer {{token}}"
      },
          {
     "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
     "raw": "{\n  \"studentId\": \"{{studentId}}\",\n  \"advisorId\": \"{{advisorId}}\"\n}"
        },
     "url": {
          "raw": "{{baseUrl}}/api/advisors/assign",
        "host": ["{{baseUrl}}"],
          "path": ["api", "advisors", "assign"]
   }
      }
    },
  {
   "name": "Remove Advisor",
      "request": {
        "method": "DELETE",
  "header": [
    {
            "key": "Authorization",
     "value": "Bearer {{token}}"
  }
        ],
      "url": {
          "raw": "{{baseUrl}}/api/advisors/remove/{{studentId}}",
          "host": ["{{baseUrl}}"],
          "path": ["api", "advisors", "remove", "{{studentId}}"]
      }
      }
    },
    {
  "name": "Get All Students",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
    "value": "Bearer {{token}}"
          }
        ],
"url": {
          "raw": "{{baseUrl}}/api/students?pageSize=100",
    "host": ["{{baseUrl}}"],
      "path": ["api", "students"],
          "query": [
          {
   "key": "pageSize",
      "value": "100"
            }
      ]
        }
      }
    },
    {
      "name": "Get Students Without Advisor",
      "request": {
        "method": "GET",
   "header": [
          {
            "key": "Authorization",
     "value": "Bearer {{token}}"
    }
        ],
        "url": {
      "raw": "{{baseUrl}}/api/students/without-advisor",
          "host": ["{{baseUrl}}"],
          "path": ["api", "students", "without-advisor"]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
    "value": "https://localhost:7175"
    },
    {
      "key": "token",
      "value": ""
    },
 {
      "key": "studentId",
      "value": ""
    },
    {
      "key": "advisorId",
      "value": ""
    }
  ]
}
```

---

## ?? Quick Start Guide

### 1. Login as Admin
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@local",
    "password": "Admin123!"
  }'
```

### 2. Get All Students
```bash
curl -X GET "https://localhost:7175/api/students?pageSize=100" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 3. Get All Advisors
```bash
curl -X GET https://localhost:7175/api/advisors \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 4. Assign Advisor
```bash
curl -X POST https://localhost:7175/api/advisors/assign \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "STUDENT_ID",
    "advisorId": "ADVISOR_ID"
  }'
```

---

## ?? Database Schema

### AppUser Table (AspNetUsers)

```sql
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) PRIMARY KEY,
    UserName NVARCHAR(256),
    Email NVARCHAR(256),
    EmailConfirmed BIT,
    PasswordHash NVARCHAR(MAX),
    -- ... other Identity fields
    AdvisorId NVARCHAR(450) NULL, -- NEW FIELD
    CONSTRAINT FK_AspNetUsers_Advisor 
FOREIGN KEY (AdvisorId) 
        REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_AspNetUsers_AdvisorId 
    ON AspNetUsers(AdvisorId);
```

---

## ?? Related Documentation

- **Complete API Guide:** [API_DOCUMENTATION.md](../API_DOCUMENTATION.md)
- **ER Diagram:** [ER_DIAGRAM.html](../ER_DIAGRAM.html)
- **README:** [README.md](../README.md)

---

**Version:** 3.0.0  
**Date:** December 20, 2024  
**Status:** ? Production Ready  
**Breaking Changes:** Yes (simplified from v2.1)
