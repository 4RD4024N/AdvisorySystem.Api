# ????? Advisor Management Endpoints (v2.1)

## ? New Student-Advisor Assignment System

**Version:** 2.1.0  
**Date:** December 20, 2024  
**Feature:** Direct student-to-advisor assignment via AppUser relationship

---

## ?? Key Changes

### Previous System (Deprecated)
- ? Advisors were assigned to **documents**
- ? Required document creation before assignment
- ? Each document could have different advisor

### New System (v2.1)
- ? Advisors assigned directly to **students**
- ? One student ? One advisor relationship
- ? Admin can search students by email/name
- ? All student's documents inherit advisor automatically

---

## ?? Advisor Endpoints

### 1. Get All Advisors

```http
GET /api/advisors
Authorization: Bearer {token}
```

**Authorization:** Any authenticated user

**Response:**
```json
[
  {
    "id": "advisor-id-456",
    "userName": "prof.smith@university.edu",
    "email": "prof.smith@university.edu"
  },
  {
    "id": "advisor-id-789",
    "userName": "prof.johnson@university.edu",
    "email": "prof.johnson@university.edu"
  }
]
```

---

### 2. ? Assign Advisor to Student (NEW)

```http
POST /api/advisors/assign-to-student
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "studentId": "student-user-id-123",
  "advisorId": "advisor-user-id-456"
}
```

**Authorization:** Requires `Admin` role

**Validation:**
- ? Student ID exists
- ? User is in "Student" role
- ? Advisor ID exists
- ? User is in "Advisor" role

**Response (Success):**
```json
{
  "message": "Öðretmen baþarýyla atandý",
  "studentName": "student@university.edu",
  "advisorName": "prof.advisor@university.edu"
}
```

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

**Side Effects:**
1. ? Student receives notification: "Öðretmen atandý"
2. ? Advisor receives notification: "Yeni öðrenci atandý"
3. ? `AppUser.AdvisorId` updated in database

**Frontend Example:**
```javascript
const assignAdvisor = async (studentId, advisorId) => {
  const token = localStorage.getItem('token');
  
  const response = await fetch('/api/advisors/assign-to-student', {
  method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      studentId: studentId,
      advisorId: advisorId
    })
  });

  if (response.ok) {
    const data = await response.json();
    alert(`? ${data.message}`);
  } else {
    const error = await response.json();
alert(`? ${error.error}`);
  }
};
```

---

### 3. ? Get My Advisor (NEW)

```http
GET /api/advisors/my-advisor
Authorization: Bearer {token}
```

**Authorization:** Any authenticated user (typically Student)

**Response (Has Advisor):**
```json
{
  "hasAdvisor": true,
  "advisor": {
    "id": "advisor-id-456",
    "userName": "prof.smith@university.edu",
    "email": "prof.smith@university.edu"
  }
}
```

**Response (No Advisor):**
```json
{
  "hasAdvisor": false,
  "advisor": null
}
```

**Frontend Example:**
```javascript
const getMyAdvisor = async () => {
  const token = localStorage.getItem('token');
  
  const response = await fetch('/api/advisors/my-advisor', {
    headers: { 'Authorization': `Bearer ${token}` }
  });

  const data = await response.json();
  
  if (data.hasAdvisor) {
  console.log('Öðretmenim:', data.advisor.userName);
 // Display advisor info in UI
    document.getElementById('advisor-name').textContent = data.advisor.userName;
  } else {
    document.getElementById('advisor-info').textContent = 'Henüz öðretmeniniz atanmamýþtýr.';
  }
};
```

---

### 4. ? Get My Students (NEW)

```http
GET /api/advisors/my-students
Authorization: Bearer {advisor-token}
```

**Authorization:** Requires `Advisor` role

**Response:**
```json
{
  "totalStudents": 15,
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

**Frontend Example:**
```javascript
const getMyStudents = async () => {
  const token = localStorage.getItem('token');

  const response = await fetch('/api/advisors/my-students', {
    headers: { 'Authorization': `Bearer ${token}` }
  });

  const data = await response.json();
  
  console.log(`Toplam ${data.totalStudents} öðrencim var`);
  
  // Display student list
  const studentList = document.getElementById('student-list');
  data.students.forEach(student => {
    const li = document.createElement('li');
    li.textContent = `${student.userName} - ${student.email}`;
    studentList.appendChild(li);
  });
};
```

---

### 5. ? Remove Advisor from Student (NEW)

```http
DELETE /api/advisors/remove-from-student/{studentId}
Authorization: Bearer {admin-token}
```

**Authorization:** Requires `Admin` role

**Response (Success):**
```json
{
  "message": "Öðretmen atamasý kaldýrýldý"
}
```

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

**Side Effects:**
- ? Student receives notification: "Öðretmen atamanýz kaldýrýldý"
- ? `AppUser.AdvisorId` set to NULL

**Frontend Example:**
```javascript
const removeAdvisor = async (studentId) => {
  if (!confirm('Öðretmen atamasýný kaldýrmak istediðinizden emin misiniz?')) {
return;
}
  
  const token = localStorage.getItem('token');
  
  const response = await fetch(`/api/advisors/remove-from-student/${studentId}`, {
    method: 'DELETE',
    headers: { 'Authorization': `Bearer ${token}` }
  });

  const data = await response.json();
  
  if (response.ok) {
    alert('? ' + data.message);
    loadStudents(); // Refresh list
  } else {
 alert('? Error: ' + data.error);
}
};
```

---

### 6. ?? Assign Advisor to Document (DEPRECATED)

```http
POST /api/advisors/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "documentId": 5,
  "advisorUserId": "advisor-id-456"
}
```

**Status:** ?? **DEPRECATED** - Use `/api/advisors/assign-to-student` instead

**Authorization:** `Admin` or `Advisor` role

**Note:** This endpoint assigns advisor to a specific document. 
**New system (v2.1)** assigns advisor to student directly via `AppUser.AdvisorId`.

**Response:**
```json
{
  "message": "Advisor assigned successfully"
}
```

---

## ?? Migration Guide

### From v1.0 to v2.1

**Database Changes:**
```sql
-- Added to AspNetUsers table
ALTER TABLE AspNetUsers ADD AdvisorId NVARCHAR(450) NULL;

-- Foreign key constraint
ALTER TABLE AspNetUsers 
ADD CONSTRAINT FK_AspNetUsers_AspNetUsers_AdvisorId 
FOREIGN KEY (AdvisorId) REFERENCES AspNetUsers(Id);
```

**Code Changes:**

**Before (v1.0):**
```javascript
// Assign advisor to document
await fetch('/api/advisors/assign', {
  method: 'POST',
  body: JSON.stringify({
    documentId: 5,
  advisorUserId: 'advisor-id'
  })
});
```

**After (v2.1):**
```javascript
// Assign advisor to student
await fetch('/api/advisors/assign-to-student', {
  method: 'POST',
  body: JSON.stringify({
    studentId: 'student-id',
    advisorId: 'advisor-id'
  })
});
```

---

## ?? Admin UI Components

### Student-Advisor Assignment Panel

```html
<div class="advisor-assignment-panel">
  <h2>Öðretmen Atama</h2>
  
  <!-- Student Search -->
  <div class="search-section">
    <input 
      type="text" 
 id="student-search" 
      placeholder="Öðrenci ara (email/isim)"
      oninput="searchStudents(this.value)"
    >
  </div>
  
  <!-- Student List -->
  <table class="student-table">
    <thead>
      <tr>
        <th>Öðrenci</th>
        <th>Email</th>
 <th>Mevcut Öðretmen</th>
        <th>Ýþlemler</th>
      </tr>
    </thead>
    <tbody id="student-tbody">
      <!-- Populated by JavaScript -->
    </tbody>
  </table>
  
  <!-- Advisor Selection Modal -->
  <div id="advisor-modal" style="display: none;">
    <h3>Öðretmen Seç</h3>
    <select id="advisor-select">
      <!-- Populated by JavaScript -->
    </select>
    <button onclick="confirmAssignment()">Ata</button>
    <button onclick="closeModal()">Ýptal</button>
  </div>
</div>

<script>
let selectedStudentId = null;

async function searchStudents(query) {
  const response = await fetch(
    `/api/students?search=${encodeURIComponent(query)}`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  
  const data = await response.json();
  
  const tbody = document.getElementById('student-tbody');
  tbody.innerHTML = '';
  
  data.students.forEach(student => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${student.userName}</td>
      <td>${student.email}</td>
      <td>${student.hasAdvisor ? student.advisor.userName : 'Yok'}</td>
      <td>
        <button onclick="openAdvisorModal('${student.id}')">
          ${student.hasAdvisor ? 'Deðiþtir' : 'Ata'}
    </button>
     ${student.hasAdvisor ? 
          `<button onclick="removeAdvisor('${student.id}')">Kaldýr</button>` : 
          ''}
      </td>
    `;
    tbody.appendChild(tr);
  });
}

async function openAdvisorModal(studentId) {
  selectedStudentId = studentId;
  
  // Load advisors
  const response = await fetch('/api/advisors', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
  const advisors = await response.json();
  
  const select = document.getElementById('advisor-select');
  select.innerHTML = '';
  
  advisors.forEach(advisor => {
    const option = document.createElement('option');
    option.value = advisor.id;
option.textContent = `${advisor.userName} - ${advisor.email}`;
    select.appendChild(option);
  });
  
  document.getElementById('advisor-modal').style.display = 'block';
}

async function confirmAssignment() {
  const advisorId = document.getElementById('advisor-select').value;
  
  const response = await fetch('/api/advisors/assign-to-student', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
    studentId: selectedStudentId,
      advisorId: advisorId
    })
  });
  
  const data = await response.json();
  
  if (response.ok) {
    alert('? ' + data.message);
    closeModal();
    searchStudents(''); // Refresh list
  } else {
    alert('? ' + data.error);
  }
}

function closeModal() {
  document.getElementById('advisor-modal').style.display = 'none';
  selectedStudentId = null;
}
</script>
```

---

## ?? Updated Student Endpoints

### GET /api/students (Updated in v2.1)

**Response now includes advisor information:**
```json
{
  "totalCount": 45,
  "students": [
    {
      "id": "student-id-123",
   "userName": "john.doe@university.edu",
      "email": "john.doe@university.edu",
      "hasAdvisor": true,
      "advisor": {
     "id": "advisor-id-456",
        "userName": "prof.smith@university.edu",
        "email": "prof.smith@university.edu"
      }
    },
    {
      "id": "student-id-456",
      "hasAdvisor": false,
      "advisor": null
    }
  ]
}
```

### GET /api/students/{id} (Updated in v2.1)

**Response now includes:**
```json
{
  "id": "student-id-123",
  "userName": "john.doe@university.edu",
  "hasAdvisor": true,
  "advisor": {
    "id": "advisor-id-456",
 "userName": "prof.smith@university.edu",
    "email": "prof.smith@university.edu"
  },
  "documents": [...],
  "submissions": [...]
}
```

### GET /api/students/without-advisor (Updated in v2.1)

**Now checks `AppUser.AdvisorId` instead of document-based advisor:**
```json
[
  {
    "id": "student-id-789",
    "userName": "student.no.advisor@university.edu",
    "email": "student.no.advisor@university.edu",
    "documentCount": 2
  }
]
```

---

## ?? Authorization Rules

### Advisor Assignment
| Role | Can Assign | Restrictions |
|------|-----------|--------------|
| **Admin** | ? Yes | Any student to any advisor |
| **Advisor** | ? No | Cannot assign |
| **Student** | ? No | Cannot assign |

### View Advisor
| Role | Can View | What They See |
|------|----------|---------------|
| **Admin** | ? All advisors | All advisor-student relationships |
| **Advisor** | ? Own students | List of assigned students |
| **Student** | ? Own advisor | Their assigned advisor |

---

## ?? Best Practices

### 1. Always Validate Roles
```javascript
// Backend validation
if (!await _userManager.IsInRoleAsync(user, "Student")) {
    return BadRequest("User is not a student");
}
```

### 2. Send Notifications
```javascript
// Notify both parties
await _notificationService.CreateNotificationAsync(
    student.Id,
    "Öðretmen Atandý",
    $"{advisor.UserName} öðretmeniniz olarak atandý.",
    NotificationType.AdvisorAssigned
);
```

### 3. Handle UI States
```javascript
if (student.hasAdvisor) {
    showChangeAdvisorButton();
    showRemoveAdvisorButton();
} else {
    showAssignAdvisorButton();
}
```

---

## ?? Testing

### Test Scenario 1: Assign Advisor
```javascript
// 1. Login as Admin
const adminToken = await login('admin@local', 'Admin123!');

// 2. Get student list
const students = await getStudents();
const student = students.find(s => s.email === 'stu@local');

// 3. Get advisor list
const advisors = await getAdvisors();
const advisor = advisors.find(a => a.email === 'ad@local');

// 4. Assign advisor
const result = await assignAdvisor(student.id, advisor.id);
expect(result.message).toBe('Öðretmen baþarýyla atandý');

// 5. Verify assignment
const updatedStudent = await getStudent(student.id);
expect(updatedStudent.hasAdvisor).toBe(true);
expect(updatedStudent.advisor.id).toBe(advisor.id);
```

### Test Scenario 2: Student Views Advisor
```javascript
// 1. Login as Student
const studentToken = await login('stu@local', 'Arda123!');

// 2. Get my advisor
const myAdvisor = await getMyAdvisor();
expect(myAdvisor.hasAdvisor).toBe(true);
expect(myAdvisor.advisor.email).toBe('ad@local');
```

### Test Scenario 3: Advisor Views Students
```javascript
// 1. Login as Advisor
const advisorToken = await login('ad@local', 'ad123!');

// 2. Get my students
const myStudents = await getMyStudents();
expect(myStudents.totalStudents).toBeGreaterThan(0);
```

---

## ?? Related Documentation

- **Full API Guide:** [API_DOCUMENTATION.md](../API_DOCUMENTATION.md)
- **Implementation Guide:** [ADVISOR_ASSIGNMENT_GUIDE.md](../ADVISOR_ASSIGNMENT_GUIDE.md)
- **Quick Summary:** [ADVISOR_ASSIGNMENT_SUMMARY.md](../ADVISOR_ASSIGNMENT_SUMMARY.md)
- **ER Diagram:** [ER_DIAGRAM.html](../ER_DIAGRAM.html)

---

**Version:** 2.1.0  
**Date:** December 20, 2024  
**Status:** ? Production Ready
