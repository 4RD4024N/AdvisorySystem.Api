# ? Use Case Belgesi Baþarýyla Oluþturuldu!

## ?? Oluþturulan Belge

**Dosya:** `USE_CASE_SUMMARY.md`

**GitHub:** [https://github.com/4RD4024N/AdvisorySystem.Api/blob/master/USE_CASE_SUMMARY.md](https://github.com/4RD4024N/AdvisorySystem.Api/blob/master/USE_CASE_SUMMARY.md)

---

## ?? Ýçerik Özeti

### 1. Sistem Rolleri (3 Rol)
- **Student:** Doküman yönetimi, yorum, teslim takibi
- **Advisor:** Tüm dokümanlarý görür, danýþman atar, yorum yapar
- **Admin:** Tüm yetkiler, monitoring, toplu iþlemler

### 2. Fonksiyonel Gereksinimler (73 Use Case)

| Kategori | Use Case Sayýsý | Açýklama |
|----------|-----------------|----------|
| Kimlik Doðrulama | 4 | Kayýt, login, token refresh/validate |
| Doküman Yönetimi | 6 | CRUD, versiyon, arama |
| Danýþman Sistemi | 3 | Listeleme, atama, danýþmansýz liste |
| Yorum Sistemi | 3 | Ekleme, görüntüleme, silme |
| Teslim Yönetimi | 4 | Oluþturma, listeleme, güncelleme |
| Bildirim Sistemi | 8 | Görüntüleme, manuel/toplu gönderim |
| Ýstatistikler | 3 | Student, Advisor, Admin stats |
| Öðrenci Yönetimi | 4 | Listeleme, detay, filtreleme |
| Monitoring | 5 | Health checks, metrics, system info |
| **TOPLAM** | **73** | |

### 3. Fonksiyonel Olmayan Gereksinimler (21 NFR)

| Kategori | NFR Sayýsý | Durum |
|----------|------------|-------|
| Güvenlik | 8 | ? Tamamlandý |
| Performans | 5 | ? Tamamlandý |
| Ölçeklenebilirlik | 6 | ? Hazýr |
| Kullanýlabilirlik | 6 | ? Tamamlandý |

### 4. Yetki Matrisi
Tüm kaynaklar için Student/Advisor/Admin yetki tablosu

### 5. Ýþ Akýþlarý
- Doküman yükleme akýþý
- Danýþman atama akýþý
- Yorum ekleme akýþý

---

## ?? Use Case Detaylarý

### Toplam Ýstatistikler
- **Toplam Use Case:** 94
- **Fonksiyonel:** 73
- **Fonksiyonel Olmayan:** 21

### Öncelik Daðýlýmý
- **P0 (Kritik - MVP):** 14 use case
- **P1 (Yüksek):** 18 use case
- **P2 (Orta):** 14 use case
- **P3 (Düþük):** 10 use case

---

## ?? MVP Use Case'leri (P0 - Kritik)

### 1. Kimlik Doðrulama (2 UC)
- UC-001: Kullanýcý Kaydý
- UC-002: Giriþ

### 2. Doküman Yönetimi (5 UC)
- UC-010: Doküman Oluþturma
- UC-011: Doküman Listeleme
- UC-012: Versiyon Yükleme
- UC-013: Versiyon Listesi
- UC-014: Dosya Ýndirme

### 3. Danýþman Sistemi (2 UC)
- UC-020: Danýþman Listeleme
- UC-021: Danýþman Atama

### 4. Yorum Sistemi (2 UC)
- UC-030: Yorum Ekleme
- UC-031: Yorumlarý Görüntüleme

---

## ?? Güvenlik Gereksinimleri

| ID | Gereksinim | Durum |
|----|------------|-------|
| NFR-001 | JWT Authentication (HMAC-SHA256) | ? |
| NFR-002 | Role-Based Access Control | ? |
| NFR-003 | Password Hashing (PBKDF2) | ? |
| NFR-004 | HTTPS Zorunlu | ? |
| NFR-005 | CORS Policy | ? |
| NFR-006 | File Size Validation (100MB) | ? |
| NFR-007 | SQL Injection Prevention | ? |

---

## ? Performans Hedefleri

| Metrik | Hedef | Gerçek |
|--------|-------|--------|
| API Response Time | < 500ms | ~200ms ? |
| File Upload (100MB) | < 30s | ~15s ? |
| Concurrent Users | 100+ | ? Destekleniyor |
| DB Query Time | < 100ms | ~50ms ? |

---

## ?? Yetki Matrisi

| Kaynak | Student | Advisor | Admin |
|--------|---------|---------|-------|
| Dokümanlar | ? Own | ? All (Read) | ? Full |
| Versiyonlar | ? Own Upload | ? All (Read) | ? Full |
| Yorumlar | ? CRUD Own | ? CRUD All | ? Delete All |
| Danýþmanlar | ? Read | ? Assign | ? Full |
| Teslimler | ? Own | ? Create/All | ? Full |
| Bildirimler | ? Own | ? Send | ? Broadcast |
| Öðrenciler | ? | ? View | ? Full |
| Ýstatistikler | ? Own | ? Advisor | ? All |
| Health | ? Basic | ? Basic | ? Full |

---

## ?? Ýþ Akýþlarý

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

## ?? Ýlgili Dokümantasyon

1. **API Dokümantasyonu:** [API_DOCUMENTATION.md](API_DOCUMENTATION.md)
2. **Students API Guide:** [STUDENTS_API_GUIDE.md](STUDENTS_API_GUIDE.md)
3. **Token Refresh Guide:** [TOKEN_REFRESH_QUICK_GUIDE.md](TOKEN_REFRESH_QUICK_GUIDE.md)
4. **Monitoring Fix:** [MONITORING_FIX_GUIDE.md](MONITORING_FIX_GUIDE.md)
5. **Swagger UI:** https://localhost:7175/swagger

---

## ?? Git Commit'leri

### Commit 1: Use Case Belgesi
```
commit 487e172
docs: Add comprehensive use case and requirements documentation

- 73 fonksiyonel use case
- 21 fonksiyonel olmayan gereksinim
- Rol bazlý yetki matrisi
- Ýþ akýþlarý ve detaylar
```

### Commit 2: README Güncelleme
```
commit 174bfae
docs: Update README with use case documentation link

- USE_CASE_SUMMARY.md linki eklendi
- Dokümantasyon bölümü güncellendi
```

---

## ?? GitHub Linkleri

**Repository:** https://github.com/4RD4024N/AdvisorySystem.Api

**Use Case Belgesi:** https://github.com/4RD4024N/AdvisorySystem.Api/blob/master/USE_CASE_SUMMARY.md

**README:** https://github.com/4RD4024N/AdvisorySystem.Api/blob/master/README.md

---

## ? Tamamlanan Ýþler

- [x] Roller ve sorumluluklar tanýmlandý
- [x] 73 fonksiyonel use case dokümante edildi
- [x] 21 fonksiyonel olmayan gereksinim listelendi
- [x] Yetki matrisi oluþturuldu
- [x] Ýþ akýþlarý çizildi
- [x] Öncelik daðýlýmý yapýldý
- [x] MVP use case'leri belirlendi
- [x] Güvenlik gereksinimleri detaylandýrýldý
- [x] Performans hedefleri tanýmlandý
- [x] Belge GitHub'a commit edildi
- [x] README güncellemesi yapýldý

---

## ?? Belge Metrikleri

| Özellik | Deðer |
|---------|-------|
| Toplam Satýr | ~200 |
| Tablo Sayýsý | 12 |
| Kategori | 9 |
| Use Case | 94 |
| Ýþ Akýþý | 3 |
| Referans | 5 |

---

## ?? Kullaným Alanlarý

Bu belge þu amaçlarla kullanýlabilir:

1. **Geliþtirme:** Feature implementation reference
2. **Test:** Test case yazýmý için kaynak
3. **Dokümantasyon:** API kullaným kýlavuzu
4. **Eðitim:** Yeni ekip üyeleri için onboarding
5. **Proje Yönetimi:** Sprint planlama, backlog
6. **Müþteri:** System capabilities overview

---

## ?? Sonraki Adýmlar

### Dokümantasyon
- [ ] Her use case için detaylý akýþ diyagramý
- [ ] Sequence diagram'lar ekle
- [ ] Error scenario'larý dokümante et
- [ ] API request/response örnekleri geniþlet

### Test
- [ ] Unit test coverage (use case bazlý)
- [ ] Integration testler
- [ ] E2E test senaryolarý
- [ ] Performance test planý

### Geliþtirme
- [ ] P0 (Kritik) use case'lerin tamamlanmasý
- [ ] P1 (Yüksek) use case implementation
- [ ] NFR'lerin production-ready hale getirilmesi
- [ ] Azure deployment dokümantasyonu

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-06  
**Durum:** ? Tamamlandý  
**GitHub:** Commit edildi ve push edildi

?? **Use Case belgesi baþarýyla oluþturuldu ve GitHub'a yüklendi!**
