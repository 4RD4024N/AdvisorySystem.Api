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

## Danýþman Ders Programý (Advisor Schedule)

Bu bölüm danýþmanlarýn öðrencilerinin ders programlarýný görüntülemesi içindir.

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/advisor-schedule/my-students` | Öðrencilerimi listele | Advisor |
| GET | `/api/advisor-schedule/student-schedule/{studentId}` | Öðrenci programý | Advisor |
| GET | `/api/advisor-schedule/all-schedules` | Tüm öðrencilerin programlarý | Advisor |
| GET | `/api/advisor-schedule/by-day/{dayOfWeek}` | Gün bazlý programlar | Advisor |
| GET | `/api/advisor-schedule/statistics` | Ýstatistikler | Advisor |

### Örnek: Öðrencilerimi Listele

```http
GET /api/advisor-schedule/my-students
Authorization: Bearer {advisor_token}
```

**Cevap:**
```json
{
  "advisorId": "advisor-guid",
  "totalStudents": 5,
  "students": [
    {
   "studentId": "student-guid",
      "email": "student1@local",
    "fullName": "Ahmet Yýlmaz",
      "studentNumber": "2021001",
      "department": "Bilgisayar Mühendisliði",
      "gpa": 3.25,
    "activeEnrollments": 6,
   "completedCourses": 12,
      "hasEnrollments": true
    }
  ]
}
```

### Örnek: Öðrenci Ders Programý

```http
GET /api/advisor-schedule/student-schedule/{studentId}
Authorization: Bearer {advisor_token}
```

**Cevap:**
```json
{
  "student": {
    "studentId": "student-guid",
    "email": "student1@local",
    "fullName": "Ahmet Yýlmaz",
    "studentNumber": "2021001",
    "department": "Bilgisayar Mühendisliði",
    "gpa": 3.25
  },
  "totalCourses": 6,
  "totalCredits": 21,
  "totalECTS": 30,
  "enrollments": [
    {
      "courseCode": "BIL101",
      "courseName": "Programlamaya Giriþ",
      "sectionCode": "A",
      "credits": 4,
      "sessions": [
        {
          "day": "Monday",
  "startTime": "09:00",
    "endTime": "10:00",
          "roomNumber": "D-201"
}
      ]
    }
  ],
  "weeklySchedule": [
    {
   "day": "Monday",
      "courses": [...]
    }
  ]
}
```

### Örnek: Gün Bazlý Program

```http
GET /api/advisor-schedule/by-day/Monday
Authorization: Bearer {advisor_token}
```

**Cevap:**
```json
{
  "day": "Monday",
  "dayNumber": 1,
  "totalStudents": 4,
  "students": [
    {
      "studentId": "student-guid",
      "fullName": "Ahmet Yýlmaz",
      "studentNumber": "2021001",
      "courses": [
   {
          "courseCode": "BIL101",
   "courseName": "Programlamaya Giriþ",
     "sessions": [
         {
     "startTime": "09:00",
   "endTime": "10:00",
      "roomNumber": "D-201"
            }
          ]
 }
      ]
    }
  ]
}
```

### Örnek: Ýstatistikler

```http
GET /api/advisor-schedule/statistics
Authorization: Bearer {advisor_token}
```

**Cevap:**
```json
{
  "totalStudents": 5,
  "studentsWithEnrollments": 4,
  "totalActiveEnrollments": 24,
  "totalCompletedCourses": 48,
  "averageCreditsPerStudent": 21.5,
  "averageGPA": 2.85,
  "popularCourses": [
    {
      "courseCode": "BIL101",
      "courseName": "Programlamaya Giriþ",
      "studentCount": 4
    }
  ]
}
```

---

## Danýþman Yönetimi (Admin)

| Method | Endpoint | Açýklama | Yetki |
|--------|----------|----------|-------|
| POST | `/api/advisors/assign-to-student` | Danýþman ata | Admin |
| DELETE | `/api/advisors/remove-from-student/{id}` | Danýþman kaldýr | Admin |

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
