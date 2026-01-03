# Ders Kayýt Sistemi

Bu dosya ders kayýt sisteminin nasýl çalýþtýðýný açýklar.

---

## Genel Bakýþ

Sistem þu iþlemleri yapar:
1. Admin ders programý oluþturur
2. Öðrenci dersleri görüntüler
3. Öðrenci derse kayýt olur
4. Sistem çakýþma kontrolü yapar
5. Sistem kapasite kontrolü yapar

---

## Ders Kayýt Akýþý

```
???????????????????????
? Öðrenci giriþ yapar ?
???????????????????????
     ?
           ?
???????????????????????
? Dersleri listeler?
? GET /schedule/available
???????????????????????
   ?
           ?
???????????????????????
? Ders seçer          ?
? POST /enroll        ?
???????????????????????
           ?
     ?????????????
     ?     ?
??????????? ???????????
? Çakýþma ? ? Kapasite?
? kontrolü? ? kontrolü?
??????????? ???????????
     ?           ?
     ?????????????
         ?
     ?????????????
     ?           ?
??????????? ???????????
? BAÞARILI? ?  HATA   ?
? Kayýt OK? ? 400 Bad ?
??????????? ???????????
```

---

## Çakýþma Kontrolü

Sistem ayný gün ve saatte iki derse kayýt olmayý engeller.

**Örnek:**
- BIL101 ? Pazartesi 09:00-10:00
- BIL102 ? Pazartesi 09:00-10:00

Bu iki derse ayný anda kayýt **olunamaz**.

**Çakýþma Formülü:**
```
Çakýþma VAR = (Ders1.Gün == Ders2.Gün) && 
     (Ders1.Baþlangýç < Ders2.Bitiþ) && 
    (Ders2.Baþlangýç < Ders1.Bitiþ)
```

**Çakýþma Yok Durumlarý:**
- Farklý günler
- Bitiþik saatler (09:00-10:00 ve 10:00-11:00)
- Ayrýk saatler

---

## Kapasite Kontrolü

Her þubenin bir kapasitesi var (genelde 50 kiþi).

- Þube doluysa kayýt **yapýlamaz**
- Sistem otomatik olarak boþ þube önerir

---

## Otomatik Þube Seçimi

Eðer öðrenci þube belirtmezse sistem:
1. Tüm þubeleri kontrol eder
2. Çakýþmayan ve boþ olan ilk þubeyi seçer
3. Bulamazsa hata döner

**Örnek Ýstek:**
```json
{
  "courseId": 5
  // sectionCode belirtilmedi
}
```

Sistem otomatik olarak uygun þubeyi seçer.

---

## API Kullanýmý

### 1. Dersleri Listele

```http
GET /api/schedule/available
Authorization: Bearer {token}
```

**Cevap:**
```json
{
  "courses": [
    {
      "courseId": 1,
  "courseCode": "BIL101",
      "courseName": "Programlamaya Giriþ",
      "sectionCode": "A",
      "enrolledCount": 35,
      "maxCapacity": 50,
      "availableSeats": 15,
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

### 2. Derse Kayýt Ol

```http
POST /api/section-enrollment/enroll
Authorization: Bearer {token}

{
  "courseId": 1,
  "sectionCode": "A"
}
```

**Baþarýlý Cevap:**
```json
{
  "message": "Enrolled successfully",
  "enrollmentId": 123,
  "sectionCode": "A",
  "courseCode": "BIL101"
}
```

### 3. Kayýtlý Derslerimi Gör

```http
GET /api/section-enrollment/my-enrollments
Authorization: Bearer {token}
```

### 4. Dersten Çýk

```http
DELETE /api/section-enrollment/unenroll/1
Authorization: Bearer {token}
```

---

## Hata Durumlarý

### Zaten Kayýtlý
```json
{
  "error": "Already enrolled in this course"
}
```

### Çakýþma Var
```json
{
  "error": "Schedule conflict detected",
  "message": "This course overlaps with your existing schedule",
  "conflictDetails": [
    {
      "courseCode": "BIL101",
      "day": "Monday",
      "existingTime": "09:00 - 10:00",
 "newTime": "09:00 - 10:00"
    }
  ]
}
```

### Þube Dolu
```json
{
  "error": "Section is full",
  "enrolledCount": 50,
  "maxCapacity": 50
}
```

### Program Yok
```json
{
  "error": "Course schedule not found",
  "message": "This course doesn't have a schedule. Please contact admin."
}
```

---

## Admin Ýþlemleri

### Ders Programý Oluþturma

Admin önce derslerin programýný oluþturmalý:

```http
POST /api/schedule/generate/1
Authorization: Bearer {admin_token}
```

Bu istek 1. dönem için otomatik program oluþturur.

---

## Özet

| Ýþlem | Endpoint | Yetki |
|-------|----------|-------|
| Dersleri listele | GET /schedule/available | Herkes |
| Kayýt ol | POST /section-enrollment/enroll | Student |
| Kayýtlarým | GET /section-enrollment/my-enrollments | Student |
| Kayýt sil | DELETE /section-enrollment/unenroll/{id} | Student |
| Program oluþtur | POST /schedule/generate/{semester} | Admin |
