# Test Dokümantasyonu

---

## Test Projesi

Proje içinde 29 adet otomatik test bulunuyor.

**Konum:** `AdvisorySystem.Tests/`

---

## Testleri Çalýþtýrma

```bash
cd AdvisorySystem.Tests
dotnet test
```

Detaylý çýktý için:
```bash
dotnet test --verbosity normal
```

---

## Test Kategorileri

### 1. Ders Kayýt Testleri (12 test)

| Test | Açýklama |
|------|----------|
| EnrollInCourse_WithValidCourse_ShouldSucceed | Normal kayýt baþarýlý |
| EnrollInCourse_AlreadyEnrolled_ShouldReturnBadRequest | Zaten kayýtlý hatasý |
| EnrollInCourse_WithScheduleConflict_ShouldReturnBadRequest | Çakýþma hatasý |
| EnrollInCourse_WithNoConflict_ShouldSucceed | Çakýþmasýz kayýt |
| EnrollInCourse_AutoSelectSection | Otomatik þube seçimi |
| EnrollInCourse_CourseNotFound | Ders bulunamadý |
| EnrollInCourse_NoSchedule | Program yok hatasý |
| Unenroll_ExistingEnrollment | Dersten çýkýþ |
| Unenroll_CompletedCourse | Tamamlanmýþ dersten çýkamaz |
| GetMyEnrollments | Kayýtlarý getir |

### 2. Çakýþma Testleri (8 test)

| Test | Açýklama |
|------|----------|
| TimeOverlap_ShouldDetectCorrectly | Zaman çakýþma formülü |
| DifferentDays_ShouldNotConflict | Farklý günler çakýþmaz |
| SameDaySameTime_ShouldConflict | Ayný gün/saat çakýþýr |
| DifferentSection_SameCourse | Farklý þubeler çakýþmaz |
| StudentCanEnrollInNonConflicting | Çakýþmasýz kayýt OK |
| StudentCannotEnrollInConflicting | Çakýþmalý kayýt engel |

### 3. Kapasite Testleri (4 test)

| Test | Açýklama |
|------|----------|
| Capacity_WhenFull_ShouldNotAllow | Dolu þubeye kayýt engel |
| Capacity_WhenHasSpace_ShouldAllow | Boþ yer varsa OK |
| AvailableSeats_ShouldDecrease | Kalan yer azalýr |
| EnrolledCount_OnlyActiveEnrollments | Sadece aktif sayýlýr |

### 4. Schedule Testleri (5 test)

| Test | Açýklama |
|------|----------|
| GenerateSchedule_ShouldCreate | Program oluþturma |
| DetectConflicts_WithOverlap | Çakýþma tespiti |
| DetectConflicts_NoOverlap | Çakýþma yok |
| HasConflict_WithExisting | Mevcut çakýþma |
| DetectConflicts_SaveToDb | DB'ye kaydet |

---

## Test Veri Seti

Testler þu verileri kullanýr:

**Dersler:**
- BIL101 - Programlamaya Giriþ (Pazartesi 09:00)
- BIL102 - Veri Yapýlarý (Pazartesi 09:00 - ÇAKIÞIYOR!)
- BIL102-B - Veri Yapýlarý B Þubesi (Salý 10:00 - çakýþmýyor)
- BIL103 - Web Programlama (Perþembe 13:00, Kapasite: 2)

**Test Senaryosu:**
1. Öðrenci BIL101'e kayýt olur ?
2. Öðrenci BIL102-A'ya kayýt olmaya çalýþýr ? ÇAKIÞMA!
3. Öðrenci BIL102-B'ye kayýt olur ? (farklý gün)
4. BIL103'e 2 kiþi kayýt olur ? ÞUBE DOLU

---

## Test Dosyalarý

```
AdvisorySystem.Tests/
??? Helpers/
?   ??? TestDbContextFactory.cs  ? Test veritabaný
??? Controllers/
?   ??? SectionEnrollmentControllerTests.cs
??? Services/
    ??? CourseSchedulerTests.cs
    ??? CapacityTests.cs
    ??? ConflictDetectionTests.cs
```

---

## Test Yazma

Yeni test eklemek için:

```csharp
[Fact]
public async Task YeniTest_Durumu_Sonuc()
{
    // Arrange - Hazýrlýk
 var context = TestDbContextFactory.Create();
    await TestDbContextFactory.SeedTestDataAsync(context);
    
// Act - Ýþlem
   var result = // bir þey yap

    // Assert - Kontrol
    Assert.True(result);
}
```

---

## Beklenen Sonuç

```
Test Çalýþtýrmasý Baþarýlý.
Toplam test sayýsý: 29
     Geçti: 29
```

Eðer bir test baþarýsýz olursa, hata mesajýný oku ve ilgili kodu düzelt.
