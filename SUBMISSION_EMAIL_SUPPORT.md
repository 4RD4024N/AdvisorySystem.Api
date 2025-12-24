# ?? Submission API Update - Email Support

**Date:** 2025-01-06  
**Version:** v3.1.1  
**Status:** ? IMPLEMENTED

---

## ?? What Changed

Submission (deadline) oluþtururken artýk **e-posta** veya **user ID** ile öðrenci belirtilebilir.

---

## ? New Feature: Email-Based Submission Creation

### Previous (v3.1.0)
```http
POST /api/submissions
{
  "studentId": "abc-123-def-456",  // ? Sadece ID
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Complete chapter 3"
}
```

### Now (v3.1.1)
```http
POST /api/submissions
{
  "studentEmail": "student1@local",  // ? YENÝ: E-posta ile
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Complete chapter 3"
}
```

**VEYA hala ID kullanabilirsiniz:**
```http
POST /api/submissions
{
  "studentId": "abc-123-def-456",  // ? Hala çalýþýyor
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Complete chapter 3"
}
```

---

## ?? API Endpoint Details

### `POST /api/submissions`

**Authorization:** `Advisor` or `Admin`

**Request Body:**

```json
{
  "studentId": "string (optional)",      // Student user ID
  "studentEmail": "string (optional)",   // Student email (NEW)
  "documentId": 5,           // Optional: Related document
  "dueDate": "2025-02-01T23:59:59Z",    // Required: Deadline
  "notes": "Complete chapters 3-5"       // Optional: Instructions
}
```

**Validation:**
- ? En az biri gerekli: `studentId` VEYA `studentEmail`
- ? Her ikisi de saðlanýrsa `studentId` önceliklidir
- ? E-posta mevcut olmak zorunda
- ? Kullanýcý "Student" rolünde olmalý
- ? Advisor sadece kendi öðrencileri için oluþturabilir

---

## ?? Test Scenarios

### Test 1: Create Submission with Email ?
```bash
POST /api/submissions
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "studentEmail": "student1@local",
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Please complete chapter 3"
}
```

**Expected Response (200 OK):**
```json
{
  "id": 15,
  "studentId": "abc-123-def-456",
  "studentEmail": "student1@local",
  "message": "Submission deadline created successfully for student1@local"
}
```

---

### Test 2: Create Submission with ID (Still Works)
```bash
POST /api/submissions
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "studentId": "abc-123-def-456",
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Complete chapter 3"
}
```

**Expected Response (200 OK):**
```json
{
  "id": 16,
  "studentId": "abc-123-def-456",
  "studentEmail": "student1@local",
  "message": "Submission deadline created successfully for student1@local"
}
```

---

### Test 3: Invalid Email
```bash
POST /api/submissions
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "studentEmail": "nonexistent@local",
  "dueDate": "2025-02-01T23:59:59Z"
}
```

**Expected Response (404 Not Found):**
```json
{
  "error": "Student not found. Please provide valid student ID or email."
}
```

---

### Test 4: Email Belongs to Non-Student
```bash
POST /api/submissions
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "studentEmail": "admin@local",  // Admin, not student
  "dueDate": "2025-02-01T23:59:59Z"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "User is not a student"
}
```

---

### Test 5: Advisor Tries Other Advisor's Student (Email)
```bash
POST /api/submissions
Authorization: Bearer {advisor1-token}
Content-Type: application/json

{
  "studentEmail": "student3@local",  // Assigned to advisor2
"dueDate": "2025-02-01T23:59:59Z"
}
```

**Expected Response (403 Forbidden):**
```json
{
  "statusCode": 403,
  "message": "Forbidden"
}
```

---

## ?? DTO Changes

### Before (v3.1.0)
```csharp
public record CreateSubmissionDto(
    string StudentId,   // Required
    int? DocumentId,
    DateTime DueDate,
    string? Notes
);
```

### After (v3.1.1)
```csharp
public record CreateSubmissionDto(
    string? StudentId,      // Optional (but one of ID/Email required)
    string? StudentEmail,   // Optional (NEW)
    int? DocumentId,
    DateTime DueDate,
    string? Notes
);
```

---

## ?? Implementation Details

### Student Lookup Logic

```csharp
// Find student by ID or Email
AppUser? student = null;

if (!string.IsNullOrEmpty(dto.StudentId))
{
    student = await _users.FindByIdAsync(dto.StudentId);
}
else if (!string.IsNullOrEmpty(dto.StudentEmail))
{
student = await _users.FindByEmailAsync(dto.StudentEmail);
}

if (student == null)
    return NotFound(new { error = "Student not found. Please provide valid student ID or email." });

// Verify user is a student
if (!await _users.IsInRoleAsync(student, "Student"))
{
    return BadRequest(new { error = "User is not a student" });
}
```

---

## ?? Use Cases

### Use Case 1: Admin Creating Deadline for Student
```javascript
// Admin knows student email, not ID
const createDeadline = async () => {
  const response = await api.post('/submissions', {
    studentEmail: 'john.doe@university.edu',
    dueDate: '2025-02-01T23:59:59Z',
    notes: 'Final project submission'
  });
  
  console.log(response.data.message);
  // "Submission deadline created successfully for john.doe@university.edu"
};
```

---

### Use Case 2: Advisor Creating Deadline from Student List
```javascript
// Advisor has student object with email
const createDeadlineForStudent = async (student) => {
  const response = await api.post('/submissions', {
    studentEmail: student.email,  // Use email directly
    dueDate: '2025-02-01T23:59:59Z',
    notes: `Complete ${student.assignedChapters}`
  });
  
  return response.data;
};
```

---

### Use Case 3: Bulk Deadline Creation by Email
```javascript
// Create deadlines for multiple students by email
const createBulkDeadlines = async (studentEmails, dueDate, notes) => {
  const results = [];
  
  for (const email of studentEmails) {
try {
      const response = await api.post('/submissions', {
studentEmail: email,
        dueDate,
        notes
});
results.push({ email, success: true, id: response.data.id });
    } catch (error) {
      results.push({ email, success: false, error: error.response?.data?.error });
    }
  }
  
  return results;
};

// Usage
const emails = ['student1@local', 'student2@local', 'student3@local'];
const results = await createBulkDeadlines(
  emails,
  '2025-02-01T23:59:59Z',
  'Complete final project'
);

console.log(results);
// [
//   { email: 'student1@local', success: true, id: 15 },
//   { email: 'student2@local', success: true, id: 16 },
//   { email: 'student3@local', success: false, error: 'Student not assigned to you' }
// ]
```

---

## ?? Security & Validation

### Email Validation
- ? E-posta veritabanýnda mevcut olmalý
- ? E-posta bir "Student" rolüne ait olmalý
- ? Advisor sadece kendi öðrencilerine eriþebilir

### Priority Order
1. **StudentId** saðlanmýþsa ? ID kullanýlýr
2. **StudentEmail** saðlanmýþsa ? Email kullanýlýr
3. **Her ikisi de saðlanmýþsa** ? StudentId öncelikli
4. **Her ikisi de yoksa** ? 404 Not Found

---

## ?? Frontend Example

### React Component

```jsx
import { useState } from 'react';
import { api } from './services/api';

const CreateDeadlineForm = () => {
  const [formData, setFormData] = useState({
    studentEmail: '',
    dueDate: '',
    notes: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await api.post('/submissions', formData);
      alert(response.data.message);
      
      // Reset form
      setFormData({ studentEmail: '', dueDate: '', notes: '' });
    } catch (err) {
      if (err.response?.status === 404) {
  setError('Student not found. Please check the email address.');
      } else if (err.response?.status === 400) {
      setError(err.response.data.error);
      } else if (err.response?.status === 403) {
        setError('You can only create deadlines for your own students.');
      } else {
   setError('Failed to create deadline. Please try again.');
      }
    } finally {
      setLoading(false);
 }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="email"
        placeholder="Student Email"
        value={formData.studentEmail}
    onChange={(e) => setFormData({ ...formData, studentEmail: e.target.value })}
        required
      />
      
   <input
 type="datetime-local"
        value={formData.dueDate}
   onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
        required
      />
      
    <textarea
        placeholder="Notes (optional)"
        value={formData.notes}
        onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
      />
      
      <button type="submit" disabled={loading}>
    {loading ? 'Creating...' : 'Create Deadline'}
      </button>
      
      {error && <div className="error">{error}</div>}
    </form>
  );
};
```

---

## ? Summary

**v3.1.1 Changes:**
- ? **Email support** added to submission creation
- ? **Backward compatible** - ID still works
- ? **Flexible** - Use ID or Email
- ? **Validated** - Student role check
- ? **Authorized** - Advisor can only create for own students

**Breaking Changes:** ? None

**Migration Required:** ? None - Fully backward compatible

---

**Status:** ? IMPLEMENTED
**Build:** ? SUCCESSFUL  
**Version:** v3.1.1

