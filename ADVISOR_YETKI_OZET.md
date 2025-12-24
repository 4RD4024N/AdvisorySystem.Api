# ? Advisor Yetkilendirme Güncellemesi - Özet

**Tarih:** 2025-01-06  
**Versiyon:** v3.1.0  
**Durum:** ? TAMAMLANDI

---

## ?? Yapýlan Deðiþiklikler

Advisor'larýn yetkiler **sadece kendi öðrencileriyle sýnýrlandýrýldý**.

---

## ? Advisor Artýk Neler Yapabilir?

### 1. Kendi Öðrencilerini Görüntüleme
```http
GET /api/students
GET /api/students/my-students
GET /api/students/{id}  # Sadece kendi öðrencileri
```

### 2. Kendi Öðrencilerine Bildirim Gönderme
```http
POST /api/students/{id}/send-notification  # Sadece kendi öðrencileri
POST /api/students/send-bulk-notification  # Sadece kendi öðrencileri
```

### 3. Kendi Öðrencilerinin Dokümanlarýný Görme
```http
GET /api/documents  # Sadece kendi öðrencilerinin
GET /api/documents/{id}/versions
GET /api/documents/download/{versionId}
GET /api/documents/preview/{versionId}
```

### 4. Kendi Öðrencilerine Submission Atama
```http
POST /api/submissions
{
  "studentId": "...",  # Sadece kendi öðrencisi
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Alt metin buraya"  # YENÝ: Notes eklenebilir
}
```

### 5. Kendi Öðrencilerinin Submissionlarýný Görme
```http
GET /api/submissions/my  # Sadece kendi öðrencileri
GET /api/students/with-pending-submissions  # Sadece kendi öðrencileri
```

---

## ? Advisor Artýk Neler Yapamaz?

1. ? Baþka advisor'larýn öðrencilerini görüntüleyemez
2. ? Baþka öðrencilere bildirim gönderemez
3. ? Baþka öðrencilerin dokümanlarýný göremez
4. ? Baþka öðrencilere submission atayamaz
5. ? Tüm öðrencilere toplu bildirim gönderemez (Admin only)
6. ? Öðretmensiz öðrenci listesini göremez (Admin only)

---

## ?? Yetkilendirme Tablosu

| Ýþlem | Admin | Advisor (v3.1) | Student |
|-------|-------|----------------|---------|
| Tüm öðrencileri görme | ? | ? **Sadece kendininkiler** | ? |
| Baþka öðrenciye bildirim | ? | ? | ? |
| Baþka öðrencinin dokümaný | ? | ? | ? |
| Baþka öðrenciye submission | ? | ? | ? |
| Toplu bildirim (tümüne) | ? | ? | ? |
| Öðretmensiz öðrenciler | ? | ? | ? |

---

## ?? Kod Deðiþiklikleri

### StudentsController.cs

**Öðrenci Listesi:**
```csharp
// Advisor sadece kendi öðrencilerini görsün
if (isAdvisor && !isAdmin)
{
    usersQuery = usersQuery.Where(u => u.AdvisorId == userId);
}
```

**Öðrenci Detayý:**
```csharp
// Advisor sadece kendi öðrencisini görebilir
if (isAdvisor && !isAdmin && student.AdvisorId != userId)
    return Forbid();
```

**Bildirim Gönderme:**
```csharp
// Advisor sadece kendi öðrencisine bildirim gönderebilir
if (isAdvisor && !isAdmin && student.AdvisorId != userId)
    return Forbid();
```

**Toplu Bildirim - Admin Only:**
```csharp
[HttpPost("send-notification-to-all")]
[Authorize(Roles = "Admin")]  // ? Sadece Admin
```

**Öðretmensizler - Admin Only:**
```csharp
[HttpGet("without-advisor")]
[Authorize(Roles = "Admin")]  // ? Sadece Admin
```

---

### DocumentsController.cs

**Doküman Listesi:**
```csharp
else if (isAdvisor)
{
  // Advisor sadece kendi öðrencilerinin dokümanlarýný görsün
    var myStudentIds = await _users.Users
        .Where(u => u.AdvisorId == uid)
        .Select(u => u.Id)
        .ToListAsync();

    query = _db.Documents.Where(d => myStudentIds.Contains(d.OwnerUserId));
}
```

**Versiyon/Ýndirme/Önizleme:**
```csharp
else if (isAdvisor)
{
    // Advisor sadece kendi öðrencilerinin dokümanlarýný görüntüleyebilir
    var student = await _users.FindByIdAsync(doc.OwnerUserId);
    if (student == null || student.AdvisorId != uid)
    {
        return Forbid();
    }
}
```

---

### SubmissionsController.cs

**Submission Listesi:**
```csharp
else if (isAdvisor)
{
    // Advisor sadece kendi öðrencilerinin submissionlarýný görsün
    var myStudentIds = await _users.Users
        .Where(u => u.AdvisorId == uid)
        .Select(u => u.Id)
        .ToListAsync();

    submissions = await _db.Submissions
        .Where(s => myStudentIds.Contains(s.StudentId))
   .OrderBy(s => s.DueDate)
    .ToListAsync();
}
```

**Submission Oluþturma:**
```csharp
// Advisor sadece kendi öðrencisi için submission oluþturabilir
if (!isAdmin && student.AdvisorId != uid)
{
    return Forbid();
}
```

**Bildirimde Notes:**
```csharp
private async Task CreateDeadlineNotification(string studentId, int submissionId, DateTime dueDate, string? notes)
{
    var message = $"You have a new submission deadline: {dueDate:dd/MM/yyyy HH:mm}";
    if (!string.IsNullOrEmpty(notes))
    {
        message += $"\n\nNotes: {notes}";  // ? Alt metin eklendi
    }
    // ...
}
```

---

## ?? Test Senaryolarý

### ? Test 1: Advisor Kendi Öðrencilerini Görebilir
```bash
# advisor1 login
GET /api/students
? Response: Sadece advisor1'e atanmýþ öðrenciler
```

### ? Test 2: Advisor Baþka Öðrencileri Göremez
```bash
GET /api/students/other-student-id
? Response: 403 Forbidden
```

### ? Test 3: Advisor Kendi Öðrencisine Bildirim Gönderebilir
```bash
POST /api/students/my-student-id/send-notification
? Response: 200 OK
```

### ? Test 4: Advisor Baþka Öðrenciye Bildirim Gönderemez
```bash
POST /api/students/other-student-id/send-notification
? Response: 403 Forbidden
```

### ? Test 5: Advisor Kendi Öðrencisine Submission Atayabilir (Notes ile)
```bash
POST /api/submissions
{
  "studentId": "my-student-id",
  "dueDate": "2025-02-01",
  "notes": "Lütfen bölüm 3'ü tamamlayýn"
}
? Response: 200 OK
? Öðrenci notification alýr (notes dahil)
```

### ? Test 6: Advisor Baþka Öðrenciye Submission Atayamaz
```bash
POST /api/submissions
{
  "studentId": "other-student-id",
  "dueDate": "2025-02-01"
}
? Response: 403 Forbidden
```

### ? Test 7: Advisor Kendi Öðrencilerinin Dokümanlarýný Görebilir
```bash
GET /api/documents
? Response: Sadece kendi öðrencilerinin dokümanlarý
```

### ? Test 8: Advisor Baþka Öðrencinin Dokümanýný Göremez
```bash
GET /api/documents/5/versions  # Document baþka öðrenciye ait
? Response: 403 Forbidden
```

---

## ?? Checklist

### Backend ?
- [x] StudentsController updated
- [x] DocumentsController updated
- [x] SubmissionsController updated
- [x] Authorization checks added
- [x] Build successful
- [x] No errors

### Documentation ?
- [x] ADVISOR_AUTHORIZATION_v3.1.md created
- [x] API_DOCUMENTATION.md updated
- [x] ADVISOR_YETKI_OZET.md created

### Frontend (Action Required) ?
- [ ] 403 error handling ekle
- [ ] "Send to All" butonu advisor için gizle
- [ ] "Without Advisor" listesi advisor için gizle
- [ ] "Sadece kendi öðrencileriniz" mesajý ekle

---

## ?? Deployment

### 1. Backend Hazýr ?
```bash
dotnet build  # ? Successful
```

### 2. Frontend Güncellemesi Gerekli ?

**Error Handling:**
```javascript
catch (error) {
  if (error.response?.status === 403) {
    toast.error('Bu iþlem için yetkiniz yok. Sadece kendi öðrencilerinize eriþebilirsiniz.');
  }
}
```

**UI Güncellemesi:**
```jsx
// Admin/Advisor ayrýmý
{user.role === 'Admin' && (
  <>
    <Button onClick={sendToAll}>Tümüne Gönder</Button>
    <Link to="/without-advisor">Öðretmensiz Öðrenciler</Link>
  </>
)}
```

---

## ?? Özet

| Özellik | Durum |
|---------|-------|
| Advisor ? Kendi öðrencileri | ? |
| Advisor ? Baþka öðrenciler | ? Engellendi |
| Advisor ? Toplu bildirim | ? Admin only |
| Advisor ? Öðretmensiz liste | ? Admin only |
| Submission notes | ? Eklendi |
| Build | ? Baþarýlý |

---

**Versiyon:** v3.1.0  
**Durum:** ? HAZIR  
**Frontend Aksiyonu:** Error handling + UI güncellemeleri

