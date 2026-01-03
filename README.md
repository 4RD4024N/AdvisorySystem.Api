# Danışmanlık Sistemi - Proje Dokümantasyonu

**Proje Adı:** AdvisorySystem.Api  
**Teknoloji:** .NET 8, Entity Framework Core, SQL Server  
**Son Güncelleme:** Ocak 2025

---

## İçindekiler

1. [Proje Hakkında](#proje-hakkında)
2. [Kurulum](#kurulum)
3. [API Kullanımı](#api-kullanımı)
4. [Veritabanı](#veritabanı)
5. [Roller ve Yetkiler](#roller-ve-yetkiler)
6. [Test](#test)

---

## Proje Hakkında

Bu sistem üniversite öğrencileri ile danışmanları arasındaki iletişimi yönetmek için geliştirilmiştir. 

**Temel Özellikler:**
- Öğrenci ders kayıt sistemi
- Çakışma kontrolü ile otomatik ders programı
- Döküman yönetimi
- Bildirim sistemi
- Danışman-öğrenci eşleştirmesi

---

## Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server (LocalDB yeterli)
- Visual Studio 2022 veya VS Code

### Adımlar

```bash
# Projeyi klonla
git clone https://github.com/4RD4024N/AdvisorySystem.Api.git
cd AdvisorySystem.Api

# Paketleri yükle
dotnet restore

# Veritabanını oluştur
dotnet ef database update

# Çalıştır
dotnet run
```

**API Adresi:** `https://localhost:7175`

---

## API Kullanımı

### Giriş Yapma

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "student1@local",
  "password": "Student123!"
}
```

**Cevap:**
```json
{
  "token": "eyJhbGciOiJIUzI1...",
  "refreshToken": "abc123...",
  "email": "student1@local",
  "role": "Student"
}
```

### Derse Kayıt Olma

```http
POST /api/section-enrollment/enroll
Authorization: Bearer {token}
Content-Type: application/json

{
  "courseId": 5,
  "sectionCode": "A"
}
```

### Ders Programımı Görme

```http
GET /api/student-courses/my-schedule
Authorization: Bearer {token}
```

### Tüm Dersleri Listeleme

```http
GET /api/schedule/available
Authorization: Bearer {token}
```

---

## Veritabanı

### Ana Tablolar

| Tablo | Açıklama |
|-------|----------|
| AspNetUsers | Kullanıcılar (öğrenci, danışman, admin) |
| Courses | Dersler |
| CourseSchedules | Ders saatleri ve şubeler |
| StudentCourseSections | Öğrenci ders kayıtları |
| Documents | Yüklenen dökümanlar |
| Notifications | Bildirimler |

### Örnek Sorgular

```sql
-- Öğrencinin kayıtlı derslerini gör
SELECT c.CourseCode, c.CourseName, scs.SectionCode
FROM StudentCourseSections scs
JOIN Courses c ON scs.CourseId = c.Id
WHERE scs.StudentId = 'öğrenci-id';

-- Ders programını gör
SELECT c.CourseCode, cs.DayOfWeek, cs.StartTime, cs.EndTime
FROM CourseSchedules cs
JOIN Courses c ON cs.CourseId = c.Id
WHERE cs.Semester = 1;
```

---

## Roller ve Yetkiler

### Öğrenci (Student)
- Kendi profilini görür/düzenler
- Derse kayıt olur/çıkar
- Ders programını görür
- Döküman yükler

### Danışman (Advisor)
- Kendi öğrencilerinin profillerini görür
- Öğrencilerine submission atar
- Rating verir
- Yorum yapar

### Admin
- Kullanıcı yönetimi
- Danışman ataması
- Ders/schedule yönetimi
- Toplu bildirim gönderme

**Not:** Admin, öğrenci işlemleri yapamaz (profil görme, derse kayıt vs.)

---

## Test

### Test Projesini Çalıştırma

```bash
cd AdvisorySystem.Tests
dotnet test
```

### Test Kategorileri

| Kategori | Test Sayısı | Açıklama |
|----------|-------------|----------|
| Enrollment | 12 | Ders kayıt testleri |
| Conflict | 8 | Çakışma kontrolü |
| Capacity | 4 | Kapasite kontrolü |
| Scheduler | 5 | Schedule oluşturma |

**Toplam:** 29 test

---

## Varsayılan Kullanıcılar

| Email | Şifre | Rol |
|-------|-------|-----|
| admin@local | Admin123! | Admin |
| advisor1@local | Advisor123! | Advisor |
| student1@local | Student123! | Student |

---

## Sorun Giderme

### CORS Hatası
`Program.cs` dosyasında frontend URL'ini ekle:
```csharp
policy.WithOrigins("http://localhost:5173")
```

### 401 Unauthorized
- Token süresi dolmuş olabilir
- Refresh token kullan veya tekrar giriş yap

### 403 Forbidden
- Yetkisiz işlem yapıyorsun
- Rol kontrolünü kontrol et

---

**Hazırlayan:** Proje Ekibi  
**Tarih:** Ocak 2025
