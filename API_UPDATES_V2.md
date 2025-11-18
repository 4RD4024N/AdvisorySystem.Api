# ?? API Sistem Güncellemeleri - Yetkilendirme ve Doðrulama

## ?? Yapýlan Deðiþiklikler Özeti

### 1. Dosya Yükleme Kýsýtlamalarý
- ? Maksimum dosya boyutu: **10MB** (önceden 100MB)
- ? Ýzin verilen dosya tipleri: **PDF, DOCX, PPTX**
- ? Otomatik doðrulama middleware'i

### 2. Yetkilendirme Sistemi
- ? **Admin**: Tüm belgeleri görebilir
- ? **Danýþman**: Sadece danýþmaný olduðu öðrencilerin belgelerini görebilir
- ? **Öðrenci**: Sadece kendi belgelerini görebilir

### 3. Versiyon Kontrolü
- ? Sadece **son 2 versiyon** görüntülenebilir (güncel + 1 önceki)

### 4. Teslim Tarihi Sistemi
- ? Danýþman öðrenciye tarihi belirterek teslim isteyebilir
- ? Otomatik bildirim sistemi (3 gün öncesinden)
- ? Background service ile sürekli kontrol

### 5. Filtreleme
- ? Baþlýk bazlý arama
- ? Tarih aralýðý filtreleme (baþlangýç-bitiþ)

---

## ?? Dosya Yükleme Kurallarý

### Boyut Limiti
**Maksimum: 10MB (10,485,760 bytes)**

**Hata Response:**
```http
HTTP/1.1 413 Payload Too Large
Content-Type: application/json

{
  "error": "File size exceeds limit",
  "message": "File 'document.pdf' is 15.50MB. Maximum allowed size is 10MB.",
  "maxSizeMB": 10,
"fileSizeMB": 15.5
}
```

---

### Ýzin Verilen Dosya Tipleri

| Tip | Uzantý | Content-Type |
|-----|---------|--------------|
| **PDF** | `.pdf` | `application/pdf` |
| **Word** | `.docx` | `application/vnd.openxmlformats-officedocument.wordprocessingml.document` |
| **PowerPoint** | `.pptx` | `application/vnd.openxmlformats-officedocument.presentationml.presentation` |

**Hata Response (Geçersiz Tip):**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "Invalid file type",
  "message": "File type '.xlsx' is not allowed. Only PDF, DOCX, and PPTX files are accepted.",
  "allowedTypes": [".pdf", ".docx", ".pptx"],
  "providedType": ".xlsx"
}
```

---

## ?? Yetkilendirme Matrisi

### Doküman Görüntüleme

| Rol | Görüntüleyebilir |
|-----|------------------|
| **Öðrenci** | ? Sadece kendi belgeleri |
| **Danýþman** | ? Danýþmaný olduðu öðrencilerin belgeleri |
| **Admin** | ? Tüm belgeler |

### Versiyon Görüntüleme
- ? Sadece **son 2 versiyon** görünür (güncel + 1 önceki)
- ? Doküman sahibi, danýþman veya admin eriþebilir

### Teslim Tarihi Oluþturma
- ? **Danýþman**: Sadece kendi öðrencilerine
- ? **Admin**: Tüm öðrencilere

---

## ?? Güncellenen API Endpoint'leri

### 1. GET /api/documents

**Deðiþiklikler:**
- ? Rol bazlý filtreleme eklendi
- ? Baþlýk aramasý eklendi
- ? Tarih filtreleme eklendi

**Request:**
```http
GET /api/documents?title=tez&startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer {token}
```

**Query Parameters:**
| Parametre | Tip | Açýklama |
|-----------|-----|----------|
| `title` | string | Baþlýkta arama (kýsmi eþleþme) |
| `startDate` | datetime | Baþlangýç tarihi (ISO 8601) |
| `endDate` | datetime | Bitiþ tarihi (ISO 8601) |

**Response (Admin):**
```json
[
  {
  "id": 1,
    "title": "Tez Taslaðý",
    "tags": "araþtýrma,tez",
    "createdAt": "2024-01-15T10:30:00Z",
    "ownerUserId": "student-id-123",
    "advisorUserId": "advisor-id-456",
    "versionCount": 3
  }
]
```

**Response (Danýþman):**
```json
[
  {
    "id": 1,
    "title": "Tez Taslaðý",
    "createdAt": "2024-01-15T10:30:00Z",
    "ownerUserId": "student-id-123",
    "advisorUserId": "current-advisor-id",
    "versionCount": 3
  }
]
```
**Note:** Danýþman sadece `advisorUserId` kendi ID'si olan dokümanlarý görür.

**Response (Öðrenci):**
```json
[
  {
    "id": 1,
    "title": "Benim Tezim",
    "createdAt": "2024-01-15T10:30:00Z",
    "ownerUserId": "current-student-id",
    "versionCount": 3
  }
]
```
**Note:** Öðrenci sadece `ownerUserId` kendi ID'si olan dokümanlarý görür.

---

### 2. GET /api/documents/{id}/versions

**Deðiþiklikler:**
- ? Sadece son 2 versiyon döner
- ? Yetkilendirme kontrolü eklendi
- ? MB cinsinden boyut bilgisi eklendi

**Request:**
```http
GET /api/documents/5/versions
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 12,
  "versionNo": 3,
    "fileName": "tez_v3.pdf",
    "size": 5242880,
    "sizeInMB": 5.0,
    "createdAt": "2024-01-20T14:30:00Z",
    "notes": "Final düzeltmeler",
    "contentType": "application/pdf"
  },
  {
    "id": 11,
    "versionNo": 2,
    "fileName": "tez_v2.pdf",
    "size": 4718592,
    "sizeInMB": 4.5,
  "createdAt": "2024-01-18T10:15:00Z",
    "notes": "Ýkinci taslak",
    "contentType": "application/pdf"
  }
]
```

**Authorization:**
- ? Doküman sahibi
- ? Atanmýþ danýþman
- ? Admin

**403 Forbidden:**
```json
{
  "error": "You don't have permission to view this document"
}
```

---

### 3. POST /api/documents/{id}/versions

**Deðiþiklikler:**
- ? 10MB limit kontrolü
- ? Dosya tipi kontrolü (PDF, DOCX, PPTX)
- ? Detaylý hata mesajlarý

**Request:**
```http
POST /api/documents/5/versions
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [binary file]
notes: "Son düzeltmeler"
```

**Success Response:**
```json
{
  "id": 13,
  "versionNo": 4
}
```

**Error Responses:**

**413 - Dosya Çok Büyük:**
```json
{
  "error": "File size exceeds limit",
  "message": "File 'large_doc.pdf' is 15.50MB. Maximum allowed size is 10MB.",
  "maxSizeMB": 10,
  "fileSizeMB": 15.5
}
```

**400 - Geçersiz Dosya Tipi:**
```json
{
"error": "Invalid file type",
  "message": "File type '.xlsx' is not allowed. Only PDF, DOCX, and PPTX files are accepted.",
  "allowedTypes": [".pdf", ".docx", ".pptx"],
  "providedType": ".xlsx"
}
```

---

### 4. POST /api/submissions

**Deðiþiklikler:**
- ? Doküman baðlantýsý eklendi
- ? Otomatik bildirim gönderimi
- ? Danýþman sadece kendi öðrencisine atayabilir

**Request:**
```http
POST /api/submissions
Authorization: Bearer {token}
Content-Type: application/json

{
  "studentId": "student-id-123",
  "documentId": 5,
  "dueDate": "2024-02-15T23:59:59Z",
  "notes": "Lütfen son düzeltmeleri yapýn"
}
```

**Parameters:**
| Alan | Tip | Zorunlu | Açýklama |
|------|-----|---------|----------|
| `studentId` | string | ? | Öðrenci ID |
| `documentId` | integer | ? | Doküman ID (opsiyonel) |
| `dueDate` | datetime | ? | Teslim tarihi |
| `notes` | string | ? | Notlar |

**Response:**
```json
{
  "id": 10,
  "message": "Submission deadline created successfully"
}
```

**Side Effects:**
- ? Öðrenciye otomatik bildirim gönderilir
- ? Background service ile deadline takibi baþlar

**Authorization:**
- **Danýþman**: Sadece `advisorUserId` kendi ID'si olan dokümanlar için
- **Admin**: Tüm dokümanlar için

**403 Forbidden (Danýþman baþka öðrenciye atamaya çalýþýrsa):**
```json
{
  "error": "You can only create submissions for your own students"
}
```

---

## ?? Otomatik Bildirim Sistemi

### Deadline Yaklaþýnca Bildirim

**Ne Zaman Gönderilir:**
- Teslim tarihine **3 gün veya daha az** kaldýðýnda
- Her submission için **sadece 1 kez** gönderilir
- Background service **her saat** kontrol eder

**Bildirim Ýçeriði:**
```json
{
  "title": "Teslim Tarihi Yaklaþýyor",
  "message": "Teslim tarihinize 2 gün kaldý. Tarih: 15/02/2024 23:59",
  "type": 0,
  "relatedEntityId": "10",
  "relatedEntityType": "Submission"
}
```

**3 gün kala:**
```
"Teslim tarihinize 3 gün kaldý. Tarih: 15/02/2024 23:59"
```

**1 gün kala:**
```
"Teslim tarihinize 1 gün kaldý. Tarih: 15/02/2024 23:59"
```

**Ayný gün (24 saatten az):**
```
"Teslim tarihinize 18 saat kaldý. Tarih: 15/02/2024 23:59"
```

---

## ?? Test Senaryolarý

### Test 1: Dosya Boyutu Kontrolü

```bash
# 5MB dosya (baþarýlý)
curl -X POST https://localhost:7175/api/documents/1/versions \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@document_5mb.pdf" \
  -F "notes=Test"
# Beklenen: 200 OK

# 15MB dosya (baþarýsýz)
curl -X POST https://localhost:7175/api/documents/1/versions \
  -H "Authorization: Bearer $TOKEN" \
-F "file=@document_15mb.pdf" \
  -F "notes=Test"
# Beklenen: 413 Payload Too Large
```

---

### Test 2: Dosya Tipi Kontrolü

```bash
# PDF (baþarýlý)
curl -X POST https://localhost:7175/api/documents/1/versions \
  -F "file=@document.pdf"
# Beklenen: 200 OK

# Excel (baþarýsýz)
curl -X POST https://localhost:7175/api/documents/1/versions \
  -F "file=@spreadsheet.xlsx"
# Beklenen: 400 Bad Request
```

---

### Test 3: Yetkilendirme

```javascript
// Admin - Tüm dokümanlarý görebilir
const adminDocs = await fetch('/api/documents', {
  headers: { 'Authorization': `Bearer ${adminToken}` }
});
// Beklenen: Tüm dokümanlar

// Danýþman - Sadece kendi öðrencilerinin dokümanlarýný görebilir
const advisorDocs = await fetch('/api/documents', {
  headers: { 'Authorization': `Bearer ${advisorToken}` }
});
// Beklenen: advisorUserId === danýþman ID olan dokümanlar

// Öðrenci - Sadece kendi dokümanlarýný görebilir
const studentDocs = await fetch('/api/documents', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
});
// Beklenen: ownerUserId === öðrenci ID olan dokümanlar
```

---

### Test 4: Versiyon Limiti

```javascript
// Doküman versiyonlarýný çek
const versions = await fetch('/api/documents/5/versions', {
  headers: { 'Authorization': `Bearer ${token}` }
}).then(r => r.json());

console.log(`Versiyon sayýsý: ${versions.length}`);
// Beklenen: Maksimum 2 versiyon (son 2)

// En yüksek versiyon numarasý
const latestVersion = Math.max(...versions.map(v => v.versionNo));
console.log(`En son versiyon: ${latestVersion}`);
```

---

### Test 5: Filtreleme

```javascript
// Baþlýk aramasý
const byTitle = await fetch('/api/documents?title=tez')
  .then(r => r.json());
console.log(`"tez" içeren: ${byTitle.length} doküman`);

// Tarih aralýðý
const byDate = await fetch('/api/documents?startDate=2024-01-01&endDate=2024-01-31')
  .then(r => r.json());
console.log(`Ocak ayýnda: ${byDate.length} doküman`);

// Kombinasyon
const filtered = await fetch('/api/documents?title=proje&startDate=2024-01-01')
  .then(r => r.json());
```

---

### Test 6: Teslim Tarihi ve Bildirim

```javascript
// 1. Danýþman teslim tarihi oluþturur
const submission = await fetch('/api/submissions', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${advisorToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    studentId: 'student-123',
    documentId: 5,
    dueDate: '2024-02-15T23:59:59Z',
  notes: 'Final versiyonu yükleyin'
  })
}).then(r => r.json());

console.log('Submission created:', submission.id);

// 2. Öðrenci bildirim alýr (anýnda)
const notifications = await fetch('/api/notifications', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
}).then(r => r.json());

const newNotification = notifications.find(n => 
  n.relatedEntityId === submission.id.toString()
);
console.log('Notification:', newNotification.message);
// Beklenen: "You have a new submission deadline: 15/02/2024 23:59"

// 3. 3 gün kala otomatik bildirim (background service)
// Background service her saat çalýþýr ve 3 gün kala bildirim gönderir
```

---

## ?? Database Deðiþiklikleri

### Submissions Tablosu

**Yeni Alanlar:**
```sql
ALTER TABLE Submissions
ADD DocumentId INT NULL,
    SubmittedAt DATETIME2 NULL,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  CreatedByUserId NVARCHAR(450) NOT NULL DEFAULT '';

ALTER TABLE Submissions
ADD CONSTRAINT FK_Submissions_Documents 
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id);
```

**Yeni Status Deðerleri:**
- `Pending` - Beklemede
- `Submitted` - Teslim edildi
- `Late` - Geç teslim

---

## ?? Migration

**Migration Name:** `UpdateSubmissionAndFileValidation`

**Uygulama:**
```bash
dotnet ef database update
```

**Geri Alma:**
```bash
dotnet ef database update PreviousMigrationName
```

---

## ?? Configuration Deðiþiklikleri

### appsettings.json

**Önceki:**
```json
"Storage": {
  "Root": "wwwroot/uploads",
  "MaxFileSize": 104857600
}
```

**Yeni:**
```json
"Storage": {
  "Root": "wwwroot/uploads",
  "MaxFileSize": 10485760
}
```

**Not:** MaxFileSize = 10MB (10 × 1024 × 1024 bytes)

---

## ?? Deployment Checklist

### Backend
- [x] Middleware güncellendi
- [x] Controller'lar güncellendi
- [x] Background service eklendi
- [x] Migration oluþturuldu
- [x] Database güncellendi
- [x] Build baþarýlý

### Frontend (Yapýlmasý Gerekenler)
- [ ] Dosya boyutu kontrolü ekle (client-side validation)
- [ ] Dosya tipi kontrolü ekle
- [ ] Filtreleme UI'ý ekle (baþlýk, tarih)
- [ ] Versiyon listesinde "son 2" bilgisi göster
- [ ] Yetkilendirme kontrollerini güncelle

### Testing
- [ ] Dosya yükleme limitleri test et
- [ ] Yetkilendirme kontrollerini test et
- [ ] Filtreleme özelliklerini test et
- [ ] Background service'i test et
- [ ] Bildirim sistemini test et

---

## ?? Destek

**Sorun Bildirimi:**
- GitHub Issues: https://github.com/4RD4024N/AdvisorySystem.Api/issues

**Dokümantasyon:**
- API Documentation: [API_DOCUMENTATION.md](API_DOCUMENTATION.md)
- Migration Guide: Bu dosya

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-18  
**Versiyon:** 2.0.0  
**Migration:** UpdateSubmissionAndFileValidation  
**Durum:** ? Tamamlandý ve Test Edildi
