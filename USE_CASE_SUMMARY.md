# ?? Advisory System - Use Case ve Gereksinimler

## ?? Sistem Rolleri

| Rol | Taným | Temel Yetkiler |
|-----|-------|----------------|
| **Student** | Doküman yükleyen öðrenci | Kendi dokümanlarýný yönetir, yorum yapar, teslimlerini takip eder |
| **Advisor** | Akademik danýþman | Tüm dokümanlarý görür, danýþman atar, yorum yapar, teslim oluþturur |
| **Admin** | Sistem yöneticisi | Tüm yetkilere sahip, istatistik görür, toplu iþlem yapar, monitoring |

---

## ?? Fonksiyonel Gereksinimler

### 1. Kimlik Doðrulama (4 UC)
- UC-001: Kullanýcý Kaydý (POST /api/auth/register)
- UC-002: Giriþ (POST /api/auth/login)
- UC-003: Token Yenileme (POST /api/auth/refresh)
- UC-004: Token Doðrulama (GET /api/auth/validate)

### 2. Doküman Yönetimi (6 UC)
- UC-010: Doküman Oluþtur (POST /api/documents)
- UC-011: Dokümanlarý Listele (GET /api/documents)
- UC-012: Versiyon Yükle (POST /api/documents/{id}/versions) - Max 100MB
- UC-013: Versiyon Listesi (GET /api/documents/{id}/versions)
- UC-014: Dosya Ýndir (GET /api/documents/download/{versionId})
- UC-015: Doküman Ara (GET /api/search/documents)

### 3. Danýþman Sistemi (3 UC)
- UC-020: Danýþman Listele (GET /api/advisors)
- UC-021: Danýþman Ata (POST /api/advisors/assign)
- UC-022: Danýþmansýz Öðrenciler (GET /api/students/without-advisor)

### 4. Yorum Sistemi (3 UC)
- UC-030: Yorum Ekle (POST /api/comments)
- UC-031: Yorumlarý Göster (GET /api/comments/version/{versionId})
- UC-032: Yorum Sil (DELETE /api/comments/{id})

### 5. Teslim Yönetimi (4 UC)
- UC-040: Teslim Oluþtur (POST /api/submissions)
- UC-041: Kendi Teslimlerim (GET /api/submissions/my)
- UC-042: Tüm Teslimler (GET /api/submissions/my) - Admin/Advisor
- UC-043: Durum Güncelle (PATCH /api/submissions/{id}/status)

### 6. Bildirim Sistemi (8 UC)
- UC-050: Bildirimlerim (GET /api/notifications)
- UC-051: Okunmamýþ Sayýsý (GET /api/notifications/unread-count)
- UC-052: Okundu Ýþaretle (PATCH /api/notifications/{id}/read)
- UC-053: Tümünü Okundu (PATCH /api/notifications/mark-all-read)
- UC-054: Bildirim Oluþtur - Admin (POST /api/notifications)
- UC-055: Öðrenciye Gönder (POST /api/students/{id}/send-notification)
- UC-056: Toplu Gönder - Admin (POST /api/students/send-bulk-notification)
- UC-057: Herkese Gönder - Admin (POST /api/students/send-notification-to-all)

**Bildirim Tipleri:**
- 0: DeadlineApproaching (Otomatik)
- 1: NewComment (Otomatik)
- 2: AdvisorAssigned (Otomatik)
- 3: DocumentUploaded
- 4: SubmissionStatusChanged
- 5: General (Manuel)

### 7. Ýstatistikler (3 UC)
- UC-060: Öðrenci Ýstatistikleri (GET /api/statistics/student/summary)
- UC-061: Danýþman Ýstatistikleri (GET /api/statistics/advisor/summary)
- UC-062: Admin Genel Bakýþ (GET /api/statistics/admin/overview)

### 8. Öðrenci Yönetimi (4 UC)
- UC-070: Öðrenci Listele (GET /api/students)
- UC-071: Öðrenci Detay (GET /api/students/{id})
- UC-072: Danýþmansýz Liste (GET /api/students/without-advisor)
- UC-073: Pending Liste (GET /api/students/with-pending-submissions)

### 9. Sistem Monitoring (5 UC)
- UC-080: Temel Saðlýk - Public (GET /api/health)
- UC-081: Detaylý Saðlýk - Admin (GET /api/health/detailed)
- UC-082: DB Kontrolü - Admin (GET /api/health/database)
- UC-083: Metrikler - Admin (GET /api/health/metrics)
- UC-084: Sistem Bilgisi - Admin (GET /api/health/system)

---

## ?? Fonksiyonel Olmayan Gereksinimler

### Güvenlik (10 NFR)
- NFR-001: JWT Kimlik Doðrulama (HMAC-SHA256) ?
- NFR-002: Role-Based Access Control ?
- NFR-003: Password Hashing (PBKDF2) ?
- NFR-004: HTTPS Zorunlu ?
- NFR-005: CORS Policy ?
- NFR-006: File Size Validation (Max 100MB) ?
- NFR-007: SQL Injection Prevention ?
- NFR-010: XSS Prevention ?? Kýsmi

### Performans (5 NFR)
- NFR-020: API Response Time < 500ms ? (~200ms)
- NFR-021: File Upload 100MB < 30s ? (~15s)
- NFR-022: DB Query Optimization ?
- NFR-023: Async/Await Pattern %100 ?
- NFR-024: Pagination (20 kayýt/sayfa) ?

### Ölçeklenebilirlik (6 NFR)
- NFR-030: Concurrent Users 100+ ?
- NFR-031: Database Scalability (Azure SQL) ?
- NFR-032: File Storage (Azure Blob) ?? Local (dev)
- NFR-033: Stateless API (JWT) ?
- NFR-034: Load Balancing Support ?
- NFR-035: Horizontal Scaling ?

### Kullanýlabilirlik (6 NFR)
- NFR-040: API Uptime 99.9% ?
- NFR-041: Error Handling (Try-catch) ?
- NFR-042: Structured Logging ?
- NFR-043: Health Checks ?
- NFR-044: API Documentation (Swagger) ?
- NFR-045: Graceful Error Messages ?

---

## ?? Yetki Matrisi

| Kaynak | Student | Advisor | Admin |
|--------|---------|---------|-------|
| **Dokümanlar** | ? Own | ? All (Read) | ? All |
| **Versiyonlar** | ? Own Upload | ? All (Read) | ? All |
| **Yorumlar** | ? CRUD Own | ? CRUD All | ? Delete All |
| **Danýþmanlar** | ? Read | ? Assign | ? Full |
| **Teslimler** | ? Own | ? Create/All | ? Full |
| **Bildirimler** | ? Own | ? Send | ? Broadcast |
| **Öðrenciler** | ? | ? View | ? Full |
| **Ýstatistikler** | ? Own | ? Advisor | ? All |
| **Health** | ? Basic | ? Basic | ? Full |

---

## ?? Temel Ýþ Akýþlarý

### Doküman Yükleme
```
1. Student ? Doküman oluþturur (POST /documents)
2. Student ? Versiyon yükler (POST /documents/{id}/versions)
3. Sistem ? Dosyayý depolar (LocalFileStorage/Azure Blob)
4. Sistem ? Version kaydý oluþturur
5. [Opsiyonel] Danýþmana bildirim
```

### Danýþman Atama
```
1. Admin/Advisor ? Doküman seçer
2. Admin/Advisor ? Danýþman seçer
3. POST /advisors/assign
4. Sistem ? Document.AdvisorUserId günceller
5. Sistem ? Öðrenciye bildirim (AdvisorAssigned)
```

### Yorum Ekleme
```
1. Kullanýcý ? Versiyonu açar
2. Kullanýcý ? Yorum yazar
3. POST /comments
4. Sistem ? Comment kaydeder
5. Sistem ? Doküman sahibine bildirim (NewComment)
```

---

## ?? Ýstatistikler

**Toplam Use Case:** 94
- Fonksiyonel: 73
- Fonksiyonel Olmayan: 21

**Öncelik Daðýlýmý:**
- P0 (Kritik): 14
- P1 (Yüksek): 18
- P2 (Orta): 14
- P3 (Düþük): 10

---

## ?? MVP Use Case'leri (P0)

1. UC-001, UC-002: Kimlik doðrulama
2. UC-010-014: Doküman yönetimi temel
3. UC-020, UC-021: Danýþman atama
4. UC-030, UC-031: Yorum sistemi temel

---

## ?? Referanslar

- [API Dokümantasyonu](API_DOCUMENTATION.md)
- [Students API Guide](STUDENTS_API_GUIDE.md)
- [Token Refresh Guide](TOKEN_REFRESH_QUICK_GUIDE.md)
- [Monitoring Fix Guide](MONITORING_FIX_GUIDE.md)
- [Swagger UI](https://localhost:7175/swagger)

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-06  
**Versiyon:** 1.0.0  
**Durum:** ? Aktif Geliþtirme
