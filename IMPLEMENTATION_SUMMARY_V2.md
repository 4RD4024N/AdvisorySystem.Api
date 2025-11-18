# ? Kapsamlý Sistem Güncellemeleri Tamamlandý

## ?? Ýstenen Özellikler

### ? 1. Yetkilendirme Sistemi
- ? **Admin**: Tüm belgeleri görebilir
- ? **Danýþman**: Sadece danýþmaný olduðu öðrencilerin belgelerini görebilir
- ? **Öðrenci**: Sadece kendi belgelerini görebilir

### ? 2. Danýþman Atama
- ? Admin'ler öðrencilere danýþman atayabilir
- ? `AdvisorsController` üzerinden atama yapýlýr

### ? 3. Versiyon Kontrolü
- ? Sadece **son 2 versiyon** görüntülenir (güncel + 1 önceki)
- ? Eski versiyonlar database'de saklanýr ama UI'da gösterilmez

### ? 4. Puanlama Sistemi
- ? 0-100 arasý puan verilebilir
- ? `DocumentRating` entity'si zaten mevcut
- ? `/api/ratings` endpoint'i kullanýlabilir

### ? 5. Teslim Tarihi Sistemi
- ? Danýþman öðrenciye tarihi belirterek teslim isteyebilir
- ? `documentId` ile belge baðlantýsý yapýlýr
- ? Sadece kendi öðrencilerine atama yapabilir

### ? 6. Otomatik Bildirimler
- ? Teslim tarihi yaklaþýnca (3 gün öncesinden) otomatik bildirim
- ? Background service her saat kontrol eder
- ? Her submission için sadece 1 kez bildirim gönderilir

### ? 7. Filtreleme
- ? Baþlýk bazlý arama (`title` parameter)
- ? Tarih aralýðý filtreleme (`startDate`, `endDate`)

### ? 8. Dosya Boyutu Limiti
- ? **10MB** maksimum dosya boyutu
- ? Middleware seviyesinde otomatik kontrol
- ? Detaylý hata mesajlarý

### ? 9. Dosya Tipi Kýsýtlamasý
- ? Sadece **PDF, DOCX, PPTX** kabul edilir
- ? Hem extension hem content-type kontrolü
- ? Detaylý hata mesajlarý

---

## ?? Yapýlan Deðiþiklikler

### Backend Deðiþiklikleri

#### 1. **Middleware Güncellendi** (`FileSizeValidationMiddleware.cs`)
```csharp
- Dosya boyutu: 100MB ? 10MB
- Dosya tipi kontrolü eklendi: PDF, DOCX, PPTX
- Extension ve content-type kontrolü
```

#### 2. **DocumentsController Güncellendi**
```csharp
- GetMine(): Rol bazlý filtreleme
- Baþlýk ve tarih filtreleme
- Versions(): Son 2 versiyon döner
```

#### 3. **SubmissionsController Güncellendi**
```csharp
- DocumentId eklendi
- Otomatik bildirim gönderimi
- Danýþman kendi öðrencilerine atama kontrolü
```

#### 4. **Background Service Eklendi** (`DeadlineNotificationService.cs`)
```csharp
- Her saat çalýþýr
- 3 gün öncesinden bildirim gönderir
- Tekrar bildirim göndermez (3 gün içinde)
```

#### 5. **Database Güncellendi** (`Submission` entity)
```csharp
+ DocumentId
+ SubmittedAt
+ Notes
+ CreatedAt
+ CreatedByUserId
```

---

### Konfigürasyon Deðiþiklikleri

#### appsettings.json
```json
"Storage": {
  "MaxFileSize": 10485760  // 10MB (önceden 100MB)
}
```

---

## ?? Yetkilendirme Matrisi

### Doküman Görüntüleme

| Rol | Görüntüleyebilir | Koþul |
|-----|------------------|-------|
| **Öðrenci** | Sadece kendi belgeleri | `ownerUserId == currentUserId` |
| **Danýþman** | Danýþmaný olduðu öðrencilerin belgeleri | `advisorUserId == currentUserId` |
| **Admin** | Tüm belgeler | Koþul yok |

### Teslim Tarihi Oluþturma

| Rol | Atayabilir | Koþul |
|-----|------------|-------|
| **Danýþman** | Sadece kendi öðrencilerine | `document.advisorUserId == currentUserId` |
| **Admin** | Tüm öðrencilere | Koþul yok |

### Versiyon Görüntüleme

| Kural | Açýklama |
|-------|----------|
| **Versiyon Sayýsý** | Son 2 versiyon (güncel + 1 önceki) |
| **Eriþim** | Doküman sahibi, danýþman veya admin |

---

## ?? Dosya Yükleme Kurallarý

### Boyut Limiti
- **Maksimum**: 10MB (10,485,760 bytes)
- **Kontrol**: Middleware seviyesinde otomatik
- **Hata Kodu**: 413 Payload Too Large

### Ýzin Verilen Tipler

| Dosya Tipi | Uzantý | MIME Type |
|------------|--------|-----------|
| PDF | `.pdf` | `application/pdf` |
| Word | `.docx` | `application/vnd.openxmlformats-officedocument.wordprocessingml.document` |
| PowerPoint | `.pptx` | `application/vnd.openxmlformats-officedocument.presentationml.presentation` |

---

## ?? Bildirim Sistemi

### Otomatik Deadline Bildirimleri

**Çalýþma Mantýðý:**
1. Background service her saat çalýþýr
2. Yaklaþan deadline'larý kontrol eder (3 gün içinde)
3. Daha önce bildirim gönderilmemiþ ise gönderir
4. Her submission için sadece 1 kez bildirim

**Bildirim Mesajlarý:**
- **3 gün kala**: "Teslim tarihinize 3 gün kaldý..."
- **1 gün kala**: "Teslim tarihinize 1 gün kaldý..."
- **Ayný gün**: "Teslim tarihinize 18 saat kaldý..."

---

## ?? Test Senaryolarý

### 1. Dosya Boyutu Testi

**5MB Dosya (Baþarýlý):**
```bash
curl -X POST https://localhost:7175/api/documents/1/versions \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@document_5mb.pdf"
# Beklenen: 200 OK
```

**15MB Dosya (Baþarýsýz):**
```bash
curl -X POST https://localhost:7175/api/documents/1/versions \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@document_15mb.pdf"
# Beklenen: 413 Payload Too Large
```

---

### 2. Dosya Tipi Testi

**PDF (Baþarýlý):**
```bash
curl -F "file=@document.pdf"
# Beklenen: 200 OK
```

**Excel (Baþarýsýz):**
```bash
curl -F "file=@spreadsheet.xlsx"
# Beklenen: 400 Bad Request
```

---

### 3. Yetkilendirme Testi

**Admin - Tüm Dokümanlar:**
```javascript
const docs = await fetch('/api/documents', {
  headers: { 'Authorization': `Bearer ${adminToken}` }
});
// Tüm dokümanlar görünür
```

**Danýþman - Kendi Öðrencileri:**
```javascript
const docs = await fetch('/api/documents', {
  headers: { 'Authorization': `Bearer ${advisorToken}` }
});
// Sadece advisorUserId === advisorId olan dokümanlar
```

**Öðrenci - Sadece Kendi Belgeleri:**
```javascript
const docs = await fetch('/api/documents', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
});
// Sadece ownerUserId === studentId olan dokümanlar
```

---

### 4. Versiyon Limiti Testi

```javascript
const versions = await fetch('/api/documents/5/versions');
const data = await versions.json();

console.log(`Versiyon sayýsý: ${data.length}`);
// Beklenen: Maksimum 2 (son 2 versiyon)
```

---

### 5. Filtreleme Testi

**Baþlýk Aramasý:**
```javascript
const filtered = await fetch('/api/documents?title=tez');
// "tez" içeren dokümanlar
```

**Tarih Aralýðý:**
```javascript
const byDate = await fetch('/api/documents?startDate=2024-01-01&endDate=2024-01-31');
// Ocak 2024'te oluþturulan dokümanlar
```

---

### 6. Teslim Tarihi ve Bildirim Testi

**1. Danýþman teslim tarihi oluþturur:**
```javascript
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
});
```

**2. Öðrenci anýnda bildirim alýr:**
```javascript
const notifications = await fetch('/api/notifications', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
});
// "You have a new submission deadline..." bildirimi
```

**3. Background service 3 gün kala bildirim gönderir:**
- Otomatik olarak her saat kontrol edilir
- 3 gün kala "Teslim tarihinize 3 gün kaldý..." bildirimi

---

## ?? Database Deðiþiklikleri

### Migration: `UpdateSubmissionAndFileValidation`

**Submissions Tablosu - Yeni Alanlar:**
```sql
ALTER TABLE Submissions ADD
  DocumentId INT NULL,
  SubmittedAt DATETIME2 NULL,
  Notes NVARCHAR(MAX) NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  CreatedByUserId NVARCHAR(450) NOT NULL DEFAULT '';

ALTER TABLE Submissions
ADD CONSTRAINT FK_Submissions_Documents 
FOREIGN KEY (DocumentId) REFERENCES Documents(Id);
```

---

## ?? API Endpoint Deðiþiklikleri

### Deðiþen Endpoint'ler

| Endpoint | Deðiþiklik | Durum |
|----------|------------|-------|
| `GET /api/documents` | Filtreleme ve yetkilendirme eklendi | ? Güncellendi |
| `GET /api/documents/{id}/versions` | Son 2 versiyon döner | ? Güncellendi |
| `POST /api/documents/{id}/versions` | 10MB + dosya tipi kontrolü | ? Güncellendi |
| `POST /api/submissions` | DocumentId ve bildirim eklendi | ? Güncellendi |

### Yeni Servisler

| Servis | Açýklama | Durum |
|--------|----------|-------|
| `DeadlineNotificationService` | Background service - Her saat deadline kontrolü | ? Eklendi |

---

## ?? Dokümantasyon

### Oluþturulan Dosyalar

1. ? **API_UPDATES_V2.md** (1500+ satýr)
 - Tüm deðiþikliklerin detaylý açýklamasý
   - Test senaryolarý
   - Frontend örnekleri
   - Hata mesajlarý

2. ? **API_DOCUMENTATION.md** (güncellendi)
   - File upload rules bölümü eklendi
   - Authorization matrix eklendi
   - Automatic notifications bölümü eklendi
   - Endpoint'ler güncellendi

---

## ?? Deployment Checklist

### Backend
- [x] Middleware güncellendi
- [x] Controller'lar güncellendi
- [x] Background service eklendi
- [x] Database migration oluþturuldu
- [x] Database güncellendi
- [x] Build baþarýlý
- [x] Commit yapýldý
- [x] GitHub'a push edildi

### Frontend (Yapýlacak)
- [ ] Dosya boyutu kontrolü ekle (client-side)
- [ ] Dosya tipi kontrolü ekle
- [ ] Filtreleme UI'ý ekle
- [ ] Versiyon listesinde "son 2" bilgisini göster
- [ ] Yetkilendirme kontrollerini uygula
- [ ] Deadline bildirimlerini göster

---

## ?? Özet

**Tamamlanan Özellikler:**
1. ? Yetkilendirme sistemi (Admin/Danýþman/Öðrenci)
2. ? Dosya boyutu limiti (10MB)
3. ? Dosya tipi kýsýtlamasý (PDF, DOCX, PPTX)
4. ? Versiyon kontrolü (son 2 versiyon)
5. ? Teslim tarihi sistemi
6. ? Otomatik bildirim sistemi
7. ? Filtreleme (baþlýk, tarih)
8. ? Puanlama sistemi (zaten mevcuttu)
9. ? Danýþman atama (zaten mevcuttu)

**Kod Ýstatistikleri:**
- Deðiþen dosyalar: 12
- Eklenen satýrlar: 1941+
- Silinen satýrlar: 261
- Yeni servis: 1 (DeadlineNotificationService)
- Yeni migration: 1 (UpdateSubmissionAndFileValidation)

**Test Durumu:**
- ? Build baþarýlý
- ? Migration uygulandý
- ? Database güncel

---

## ?? Destek

**GitHub:** https://github.com/4RD4024N/AdvisorySystem.Api  
**Commit:** `345d63e`  
**Branch:** master  
**Migration:** UpdateSubmissionAndFileValidation

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-18  
**Versiyon:** 2.0.0  
**Durum:** ? Tüm Ýstenen Özellikler Tamamlandý
