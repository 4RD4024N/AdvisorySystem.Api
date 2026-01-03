# API Endpoint Listesi

Bu dosya tüm API endpoint'lerini ve nasýl kullanýlacaðýný açýklar.

---

## Kimlik Doðrulama (Auth)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| POST | `/api/auth/register` | Yeni kullanýcý kaydý | Herkes |
| POST | `/api/auth/login` | Giriþ yap | Herkes |
| POST | `/api/auth/refresh` | Token yenile | Herkes |
| POST | `/api/auth/logout` | Çýkýþ yap | Giriþ yapmýþ |

### Örnek: Giriþ

```http
POST /api/auth/login
{
  "email": "student1@local",
  "password": "Student123!"
}
```

---

## Ders Kayýt (Section Enrollment)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/section-enrollment/my-enrollments` | Kayýtlý derslerim | Student |
| POST | `/api/section-enrollment/enroll` | Derse kayýt ol | Student |
| DELETE | `/api/section-enrollment/unenroll/{courseId}` | Dersten çýk | Student |

### Örnek: Derse Kayýt

```http
POST /api/section-enrollment/enroll
Authorization: Bearer {token}

{
  "courseId": 5,
  "sectionCode": "A"
}
```

**Olasý Hatalar:**
- `400 Already enrolled` - Zaten kayýtlýsýn
- `400 Schedule conflict` - Baþka dersle çakýþýyor
- `400 Section is full` - Þube dolu
- `400 Course schedule not found` - Ders programý yok

---

## Ders Programý (Schedule)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/schedule/available` | Tüm dersleri listele | Giriþ yapmýþ |
| GET | `/api/schedule/semester/{semester}` | Dönem bazlý dersler | Giriþ yapmýþ |
| POST | `/api/schedule/generate/{semester}` | Program oluþtur | Admin |
| DELETE | `/api/schedule/semester/{semester}` | Programý sil | Admin |

### Örnek: Tüm Dersleri Listele

```http
GET /api/schedule/available
Authorization: Bearer {token}
```

**Cevap:**
```json
{
  "totalCourses": 45,
  "courses": [
    {
      "courseId": 1,
    "courseCode": "BIL101",
      "courseName": "Programlamaya Giriþ",
      "sectionCode": "A",
      "credits": 4,
   "isFull": false,
      "schedule": [
        {
          "dayOfWeek": "Monday",
          "startTime": "09:00",
        "endTime": "10:00"
        }
      ]
    }
  ]
}
```

---

## Öðrenci Dersleri (Student Courses)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/student-courses/my-program` | Aldýðým dersler | Student |
| GET | `/api/student-courses/my-schedule` | Haftalýk programým | Student |
| POST | `/api/student-courses/enroll` | Derse kayýt | Student |
| DELETE | `/api/student-courses/{id}` | Dersten çýk | Student |

---

## Dersler (Courses)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/courses` | Tüm dersler | Giriþ yapmýþ |
| GET | `/api/courses/{id}` | Ders detayý | Giriþ yapmýþ |
| POST | `/api/courses` | Ders ekle | Admin |
| PUT | `/api/courses/{id}` | Ders güncelle | Admin |
| DELETE | `/api/courses/{id}` | Ders sil | Admin |

---

## Profil (Student Profile)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/studentprofile/me` | Kendi profilim | Student |
| POST | `/api/studentprofile` | Profil oluþtur | Student |
| PUT | `/api/studentprofile` | Profil güncelle | Student |

---

## Dökümanlar (Documents)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/documents` | Dökümanlarým | Giriþ yapmýþ |
| POST | `/api/documents` | Döküman yükle | Student |
| GET | `/api/documents/{id}/versions` | Versiyonlar | Giriþ yapmýþ |
| GET | `/api/documents/download/{versionId}` | Ýndir | Giriþ yapmýþ |

---

## Bildirimler (Notifications)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/notifications` | Bildirimlerim | Giriþ yapmýþ |
| PATCH | `/api/notifications/{id}/read` | Okundu iþaretle | Giriþ yapmýþ |
| POST | `/api/students/{id}/send-notification` | Bildirim gönder | Admin |

---

## Danýþman (Advisor)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/advisor-schedule/my-students` | Öðrencilerim | Advisor |
| GET | `/api/advisor-schedule/student-schedule/{id}` | Öðrenci programý | Advisor |
| POST | `/api/advisors/assign-to-student` | Danýþman ata | Admin |

---

## Hata Kodlarý

| Kod | Anlamý |
|-----|--------|
| 200 | Baþarýlý |
| 201 | Oluþturuldu |
| 400 | Hatalý istek |
| 401 | Giriþ yapýlmamýþ |
| 403 | Yetkisiz |
| 404 | Bulunamadý |
| 500 | Sunucu hatasý |

---

## Token Kullanýmý

Her istekte header'a token ekle:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Token süresi dolunca refresh token ile yenile:

```http
POST /api/auth/refresh
{
  "refreshToken": "abc123..."
}
```
