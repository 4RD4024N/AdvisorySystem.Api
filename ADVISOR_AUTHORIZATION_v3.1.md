# 🔐 Advisor Authorization Update - v3.1

**Date:** 2025-01-06  
**Version:** 3.1.0  
**Status:** ✅ IMPLEMENTED

---

## 🎯 Changes Overview

Advisor yetkiler artık kısıtlandı. Advisorlar **sadece kendi öğrencileriyle** ilgili işlemler yapabilir.

---

## ✅ Advisor Yetkileri (v3.1)

### 📋 Ne Yapabilir:

1. ✅ **Kendi öğrencilerini görüntüleme**
   - `GET /api/students` - Sadece kendi öğrencileri
   - `GET /api/students/my-students` - Atanmış öğrenciler listesi
   - `GET /api/students/{id}` - Kendi öğrencisinin detayları

2. ✅ **Kendi öğrencilerine bildirim gönderme**
   - `POST /api/students/{id}/send-notification`
   - `POST /api/students/send-bulk-notification` - Sadece kendi öğrencileri

3. ✅ **Kendi öğrencilerinin dokümanlarını görüntüleme**
   - `GET /api/documents` - Sadece kendi öğrencilerinin dokümanları
   - `GET /api/documents/{id}/versions` - Kendi öğrencilerinin versiyonları
   - `GET /api/documents/download/{versionId}` - İndirme
   - `GET /api/documents/preview/{versionId}` - PDF önizleme
   - `GET /api/documents/metadata/{versionId}` - Metadata

4. ✅ **Kendi öğrencilerine submission (teslim tarihi) atama**
   - `POST /api/submissions` - Sadece kendi öğrencileri için
   - Notes ekleyebilir, deadline belirleyebilir

5. ✅ **Kendi öğrencilerinin submissionlarını görme**
   - `GET /api/submissions/my` - Sadece kendi öğrencilerinin
   - `GET /api/students/with-pending-submissions` - Kendi öğrencileri

### ❌ Ne Yapamaz:

1. ❌ Başka advisorların öğrencilerini görüntüleme
2. ❌ Başka öğrencilere bildirim gönderme
3. ❌ Başka öğrencilerin dokümanlarını görme
4. ❌ Başka öğrencilere submission atama
5. ❌ Tüm öğrencilere toplu bildirim gönderme (Admin only)
6. ❌ Advisor atanmamış öğrencileri görme (Admin only)

---

## 📊 API Endpoint Değişiklikleri

### Students Endpoints

#### `GET /api/students`
```http
GET /api/students?search=john&page=1&pageSize=20
Authorization: Bearer {advisor-token}
```

**Önce (v3.0):**
- Admin: Tüm öğrenciler
- Advisor: Tüm öğrenciler ✅

**Sonra (v3.1):**
- Admin: Tüm öğrenciler
- Advisor: **Sadece kendi öğrencileri** ✅

**Response:**
```json
{
  "totalCount": 2,
  "students": [
    {
      "id": "student-1",
      "userName": "student1@local",
      "hasAdvisor": true,
      "advisor": {
        "id": "advisor-id",
        "userName": "advisor1@local"
   }
    }
  ]
}
```

---

#### `GET /api/students/{id}`
```http
GET /api/students/student-123
Authorization: Bearer {advisor-token}
```

**Yetki Kontrolü:**
```csharp
// Advisor can only view their own students
if (isAdvisor && !isAdmin && student.AdvisorId != userId)
    return Forbid(); // ← YENİ
```

**403 Response:**
```json
{
  "statusCode": 403,
  "message": "Forbidden"
}
```

---

#### `POST /api/students/{id}/send-notification`
```http
POST /api/students/student-123/send-notification
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "title": "Document Review",
  "message": "Please revise section 3",
  "type": 5
}
```

**Yetki Kontrolü:**
```csharp
// Advisor can only send notifications to their own students
if (isAdvisor && !isAdmin && student.AdvisorId != userId)
  return Forbid(); // ← YENİ
```

---

#### `POST /api/students/send-bulk-notification`
```http
POST /api/students/send-bulk-notification
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "studentIds": ["student-1", "student-2", "student-3"],
  "title": "Group Meeting",
  "message": "Meeting tomorrow at 10 AM",
  "type": 5
}
```

**Yetki Kontrolü:**
```csharp
foreach (var studentId in dto.StudentIds)
{
    var student = await _userManager.FindByIdAsync(studentId);
    
    // Advisor can only send to their own students
    if (isAdvisor && !isAdmin && student.AdvisorId != userId)
    {
        failedCount++;
        errors.Add($"Student {studentId} is not assigned to you"); // ← YENİ
    continue;
    }
}
```

**Response:**
```json
{
  "message": "Notification sent to 2 students",
  "successCount": 2,
  "failedCount": 1,
  "errors": [
    "Student student-3 is not assigned to you"
  ]
}
```

---

#### `POST /api/students/send-notification-to-all`
```http
POST /api/students/send-notification-to-all
Authorization: Bearer {admin-token}
```

**DEĞİŞİKLİK:**
- ❌ Advisor artık **kullanamaz**
- ✅ **Sadece Admin**

```csharp
[HttpPost("send-notification-to-all")]
[Authorize(Roles = "Admin")] // ← Admin ONLY
```

---

#### `GET /api/students/without-advisor`
```http
GET /api/students/without-advisor
Authorization: Bearer {admin-token}
```

**DEĞİŞİKLİK:**
- ❌ Advisor artık **kullanamaz**
- ✅ **Sadece Admin**

```csharp
[HttpGet("without-advisor")]
[Authorize(Roles = "Admin")] // ← Admin ONLY
```

---

#### `GET /api/students/with-pending-submissions`
```http
GET /api/students/with-pending-submissions
Authorization: Bearer {advisor-token}
```

**Önce (v3.0):**
- Admin: Tüm öğrenciler
- Advisor: Tüm öğrenciler

**Sonra (v3.1):**
- Admin: Tüm öğrenciler
- Advisor: **Sadece kendi öğrencileri** ✅

```csharp
// Advisor can only see their own students' submissions
if (isAdvisor && !isAdmin)
{
    var myStudentIds = await _userManager.Users
    .Where(u => u.AdvisorId == userId)
    .Select(u => u.Id)
.ToListAsync();

    submissionsQuery = submissionsQuery.Where(s => myStudentIds.Contains(s.StudentId));
}
```

---

### Documents Endpoints

#### `GET /api/documents`
```http
GET /api/documents?title=thesis
Authorization: Bearer {advisor-token}
```

**DEĞİŞİKLİK:**

```csharp
// Önce (v3.0)
if (isAdvisor)
{
    query = _db.Documents.Where(d => d.AdvisorUserId == uid); // Eski: advisor field
}

// Sonra (v3.1)
if (isAdvisor)
{
    var myStudentIds = await _users.Users
        .Where(u => u.AdvisorId == uid)
        .Select(u => u.Id)
        .ToListAsync();

    query = _db.Documents.Where(d => myStudentIds.Contains(d.OwnerUserId)); // Yeni: student ownership
}
```

**Açıklama:** Artık `Document.AdvisorUserId` kullanmıyoruz, `AppUser.AdvisorId` ile belirleniyor.

---

#### `GET /api/documents/{id}/versions`
```http
GET /api/documents/5/versions
Authorization: Bearer {advisor-token}
```

**Yetki Kontrolü:**
```csharp
else if (isAdvisor)
{
    // Advisor can only see their students' documents
    var student = await _users.FindByIdAsync(doc.OwnerUserId);
    if (student == null || student.AdvisorId != uid)
  {
        return Forbid(); // ← YENİ
    }
}
```

---

#### `GET /api/documents/download/{versionId}`
```http
GET /api/documents/download/12
Authorization: Bearer {advisor-token}
```

**Yetki Kontrolü:**
```csharp
else if (isAdvisor)
{
    // Advisor can only download their students' documents
    var student = await _users.FindByIdAsync(v.Document.OwnerUserId);
    if (student == null || student.AdvisorId != uid)
    {
   return Forbid(); // ← YENİ
    }
}
```

---

#### `GET /api/documents/preview/{versionId}`
```http
GET /api/documents/preview/12
Authorization: Bearer {advisor-token}
```

**Yetki Kontrolü:** Aynı `download` ile

---

#### `GET /api/documents/metadata/{versionId}`
```http
GET /api/documents/metadata/12
Authorization: Bearer {advisor-token}
```

**Yetki Kontrolü:** Aynı `download` ile

---

### Submissions Endpoints

#### `GET /api/submissions/my`
```http
GET /api/submissions/my
Authorization: Bearer {advisor-token}
```

**DEĞİŞİKLİK:**

```csharp
// Önce (v3.0)
if (isAdmin || isAdvisor)
{
    // Admin/Advisor can see all submissions
submissions = await _db.Submissions.ToListAsync(); // Tümü
}

// Sonra (v3.1)
else if (isAdvisor)
{
    // Advisor can only see their students' submissions
    var myStudentIds = await _users.Users
  .Where(u => u.AdvisorId == uid)
  .Select(u => u.Id)
        .ToListAsync();

    submissions = await _db.Submissions
        .Where(s => myStudentIds.Contains(s.StudentId)) // Sadece kendi öğrencileri
        .OrderBy(s => s.DueDate)
        .ToListAsync();
}
```

---

#### `POST /api/submissions`
```http
POST /api/submissions
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "studentId": "student-123",
  "documentId": 5,
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Please complete chapters 3-5"
}
```

**Yetki Kontrolü:**
```csharp
// Check if student exists
var student = await _users.FindByIdAsync(dto.StudentId);
if (student == null)
    return NotFound(new { error = "Student not found" });

// Advisor can only create submissions for their own students
if (!isAdmin && student.AdvisorId != uid)
{
    return Forbid(); // ← YENİ
}
```

**403 Response:**
```json
{
  "statusCode": 403,
  "message": "Forbidden"
}
```

**Bildirim:**
- Notes eklenirse bildirimde gösterilir
```csharp
var message = $"You have a new submission deadline: {dueDate:dd/MM/yyyy HH:mm}";
if (!string.IsNullOrEmpty(notes))
{
    message += $"\n\nNotes: {notes}"; // ← Notes eklendi
}
```

---

## 🔒 Authorization Summary

### Endpoint Authorization Matrix

| Endpoint | Admin | Advisor | Student |
|----------|-------|---------|---------|
| **Students** |
| GET /api/students | All students | **Own students only** ✨ | ❌ |
| GET /api/students/{id} | Any student | **Own students only** ✨ | Own profile |
| POST /api/students/{id}/send-notification | Any student | **Own students only** ✨ | ❌ |
| POST /api/students/send-bulk-notification | Any students | **Own students only** ✨ | ❌ |
| POST /api/students/send-notification-to-all | ✅ | ❌ **Changed** | ❌ |
| GET /api/students/without-advisor | ✅ | ❌ **Changed** | ❌ |
| GET /api/students/with-pending-submissions | All | **Own students** ✨ | ❌ |
| GET /api/students/my-students | ❌ | ✅ Own students | ❌ |
| **Documents** |
| GET /api/documents | All | **Own students'** ✨ | Own docs |
| GET /api/documents/{id}/versions | All | **Own students'** ✨ | Own docs |
| GET /api/documents/download/{id} | All | **Own students'** ✨ | Own docs |
| GET /api/documents/preview/{id} | All | **Own students'** ✨ | Own docs |
| GET /api/documents/metadata/{id} | All | **Own students'** ✨ | Own docs |
| **Submissions** |
| GET /api/submissions/my | All | **Own students'** ✨ | Own |
| POST /api/submissions | Any student | **Own students only** ✨ | ❌ |

**✨ Changed in v3.1**

---

## 🧪 Testing Guide

### Test 1: Advisor Can See Own Students

```bash
# Login as advisor1
POST /api/auth/login
{
  "email": "advisor1@local",
  "password": "Advisor123!"
}

# Get my students
GET /api/students
Authorization: Bearer {advisor1-token}

# Expected: Only students where AdvisorId = advisor1's ID
```

---

### Test 2: Advisor Cannot See Other Students

```bash
# Try to get student assigned to another advisor
GET /api/students/other-student-id
Authorization: Bearer {advisor1-token}

# Expected: 403 Forbidden
```

---

### Test 3: Advisor Can Send Notification to Own Students

```bash
POST /api/students/my-student-id/send-notification
Authorization: Bearer {advisor1-token}
{
  "title": "Test",
  "message": "Hello",
  "type": 5
}

# Expected: 200 OK
```

---

### Test 4: Advisor Cannot Send to Other Students

```bash
POST /api/students/other-student-id/send-notification
Authorization: Bearer {advisor1-token}
{
  "title": "Test",
  "message": "Hello",
  "type": 5
}

# Expected: 403 Forbidden
```

---

### Test 5: Advisor Can Create Submission for Own Student

```bash
POST /api/submissions
Authorization: Bearer {advisor1-token}
{
  "studentId": "my-student-id",
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Complete chapter 3"
}

# Expected: 200 OK
# Student receives notification with notes
```

---

### Test 6: Advisor Cannot Create Submission for Other Student

```bash
POST /api/submissions
Authorization: Bearer {advisor1-token}
{
  "studentId": "other-student-id",
  "dueDate": "2025-02-01T23:59:59Z"
}

# Expected: 403 Forbidden
```

---

### Test 7: Advisor Can View Own Students' Documents

```bash
GET /api/documents
Authorization: Bearer {advisor1-token}

# Expected: Only documents of students where AdvisorId = advisor1's ID
```

---

### Test 8: Advisor Cannot Access Other Students' Documents

```bash
GET /api/documents/5/versions
Authorization: Bearer {advisor1-token}

# (Document belongs to student assigned to advisor2)
# Expected: 403 Forbidden
```

---

## 📋 Migration Checklist

### Backend ✅
- [x] Update StudentsController authorization
- [x] Update DocumentsController authorization  
- [x] Update SubmissionsController authorization
- [x] Build successful
- [x] Tests passing

### Database ❌
- No migration needed

### Frontend (Action Required)
- [ ] Update error handling for 403 responses
- [ ] Remove "Send to All" button for Advisors
- [ ] Remove "Without Advisor" list for Advisors
- [ ] Add "You can only access your own students" message

---

## 🐛 Error Handling

### Common 403 Errors

**Advisor trying to access other student:**
```json
{
  "statusCode": 403,
  "message": "Forbidden"
}
```

**Frontend Handling:**
```javascript
try {
  const response = await api.get(`/students/${studentId}`);
} catch (error) {
  if (error.response?.status === 403) {
    toast.error('You can only access students assigned to you');
  }
}
```

---

## 📖 Documentation Updates

### API_DOCUMENTATION.md
- ✅ Updated authorization requirements
- ✅ Added v3.1 notes
- ✅ Updated examples with authorization

### README.md
- ✅ Updated feature list
- ✅ Added v3.1 changelog
- ✅ Updated authorization matrix

---

## ✅ Summary

**v3.1 Changes:**
1. ✅ Advisors **sadece kendi öğrencilerine** erişebilir
2. ✅ Toplu bildirim/rapor endpoints **Admin-only**
3. ✅ Tüm document/submission işlemleri **kısıtlı**
4. ✅ Submission notes özelliği **eklendi**
5. ✅ Build successful, tests passing

**Breaking Changes:** ❌ None (only permission restrictions)

**Action Required:**
- Frontend: 403 error handling
- Frontend: UI güncellemeleri (Admin/Advisor ayrımı)

---

**Status:** ✅ IMPLEMENTED  
**Build:** ✅ SUCCESSFUL  
**Ready for Testing:** ✅ YES

