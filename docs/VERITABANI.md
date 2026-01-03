# Veritabaný Þemasý

Bu dosya veritabaný tablolarýný ve iliþkilerini açýklar.

---

## Tablo Listesi

### Kullanýcýlar

**AspNetUsers** - Tüm kullanýcýlar
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | string | Kullanýcý ID |
| Email | string | E-posta |
| UserName | string | Kullanýcý adý |
| AdvisorId | string? | Danýþman ID (öðrenciler için) |

**AspNetRoles** - Roller
- Admin
- Advisor  
- Student

**StudentProfiles** - Öðrenci profilleri
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| UserId | string | Kullanýcý ID |
| FirstName | string | Ad |
| LastName | string | Soyad |
| StudentNumber | string | Öðrenci no |
| Department | string | Bölüm |
| GPA | double? | Not ortalamasý |

---

### Dersler

**Courses** - Dersler
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| CourseCode | string | Ders kodu (BIL101) |
| CourseName | string | Ders adý |
| TheoryHours | int | Teori saati |
| PracticeHours | int | Uygulama saati |
| Credits | int | Kredi |
| ECTS | int | AKTS |
| Semester | int? | Dönem |
| IsElective | bool | Seçmeli mi |
| CategoryId | int | Kategori |

**CourseCategories** - Ders kategorileri
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| Name | string | Kategori adý |
| DisplayOrder | int | Sýralama |

**CourseSchedules** - Ders programý
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| CourseId | int | Ders ID |
| Semester | int | Dönem |
| SectionCode | string | Þube (A, B, C) |
| DayOfWeek | int | Gün (0=Pazar, 1=Pazartesi...) |
| StartTime | TimeSpan | Baþlangýç saati |
| EndTime | TimeSpan | Bitiþ saati |
| RoomNumber | string | Derslik |
| InstructorName | string | Öðretmen |
| MaxCapacity | int | Kapasite |

---

### Öðrenci Kayýtlarý

**StudentCourseSections** - Öðrenci ders kayýtlarý
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| StudentId | string | Öðrenci ID |
| CourseId | int | Ders ID |
| SectionCode | string | Þube |
| IsCompleted | bool | Tamamlandý mý |
| EnrolledAt | DateTime | Kayýt tarihi |

**StudentCourses** - Eski kayýt tablosu (geriye uyumluluk)
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| StudentId | string | Öðrenci ID |
| CourseId | int | Ders ID |
| Grade | double? | Not |
| LetterGrade | string | Harf notu |

---

### Dökümanlar

**Documents** - Dökümanlar
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| Title | string | Baþlýk |
| OwnerUserId | string | Sahibi |
| AdvisorUserId | string? | Danýþman |
| Tags | string | Etiketler |

**DocumentVersions** - Döküman versiyonlarý
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| DocumentId | int | Döküman ID |
| VersionNo | int | Versiyon numarasý |
| FileName | string | Dosya adý |
| StoragePath | string | Depolama yolu |

---

### Diðer

**Notifications** - Bildirimler
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| UserId | string | Kullanýcý ID |
| Title | string | Baþlýk |
| Message | string | Mesaj |
| IsRead | bool | Okundu mu |

**Submissions** - Teslimler
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| StudentId | string | Öðrenci ID |
| DocumentId | int? | Döküman ID |
| DueDate | DateTime | Son tarih |
| Status | string | Durum |

**Comments** - Yorumlar
| Kolon | Tip | Açýklama |
|-------|-----|----------|
| Id | int | ID |
| DocumentVersionId | int | Versiyon ID |
| AuthorUserId | string | Yazar |
| Content | string | Ýçerik |

---

## Ýliþkiler

```
AspNetUsers (1) ???? (N) StudentProfiles
AspNetUsers (1) ???? (N) StudentCourseSections
AspNetUsers (1) ???? (N) Documents
AspNetUsers (1) ???? (N) Notifications

Courses (1) ???? (N) CourseSchedules
Courses (1) ???? (N) StudentCourseSections
CourseCategories (1) ???? (N) Courses

Documents (1) ???? (N) DocumentVersions
DocumentVersions (1) ???? (N) Comments
```

---

## Örnek Sorgular

### Öðrencinin Ders Programý
```sql
SELECT 
    c.CourseCode,
    c.CourseName,
    cs.DayOfWeek,
 cs.StartTime,
    cs.EndTime,
    cs.RoomNumber
FROM StudentCourseSections scs
JOIN Courses c ON scs.CourseId = c.Id
JOIN CourseSchedules cs ON cs.CourseId = c.Id AND cs.SectionCode = scs.SectionCode
WHERE scs.StudentId = 'student-id'
ORDER BY cs.DayOfWeek, cs.StartTime;
```

### Boþ Kalan Dersler
```sql
SELECT 
    c.CourseCode,
    cs.SectionCode,
    cs.MaxCapacity - COUNT(scs.Id) AS AvailableSeats
FROM CourseSchedules cs
JOIN Courses c ON cs.CourseId = c.Id
LEFT JOIN StudentCourseSections scs ON scs.CourseId = cs.CourseId AND scs.SectionCode = cs.SectionCode
GROUP BY c.CourseCode, cs.SectionCode, cs.MaxCapacity
HAVING cs.MaxCapacity - COUNT(scs.Id) > 0;
```

### Danýþmanýn Öðrencileri
```sql
SELECT 
    u.Email,
    sp.FirstName,
    sp.LastName,
    sp.StudentNumber
FROM AspNetUsers u
JOIN StudentProfiles sp ON sp.UserId = u.Id
WHERE u.AdvisorId = 'advisor-id';
```
