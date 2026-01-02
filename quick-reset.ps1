# Hýzlý Ders Silme Scripti
# Sadece SQL komutlarýný çalýþtýrýr, uygulamayý manuel baþlatmanýz gerekir

Write-Host ""
Write-Host "???  Dersler siliniyor..." -ForegroundColor Yellow
Write-Host ""

$sql = @"
USE AdvisorySystemDB;
DELETE FROM StudentCourseSections;
DELETE FROM StudentCourses;
DELETE FROM CourseSchedules;
DELETE FROM Prerequisites;
DELETE FROM Courses;
DELETE FROM CourseCategories;
PRINT 'Tüm dersler silindi!';
"@

try {
    sqlcmd -S "(localdb)\MSSQLLocalDB" -Q $sql
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "? Baþarýlý! Dersler silindi." -ForegroundColor Green
        Write-Host ""
   Write-Host "ÞÝMDÝ YAPMANIZ GEREKENLER:" -ForegroundColor Cyan
      Write-Host "1??  dotnet run        (Uygulamayý baþlat)" -ForegroundColor White
  Write-Host "2??  Ctrl+C tuþuna bas (Seeding tamamlandýktan sonra)" -ForegroundColor White
  Write-Host "3??  SQL'de kontrol et: SELECT COUNT(*), COUNT(Description) FROM Courses" -ForegroundColor White
        Write-Host ""
    }
} catch {
    Write-Host "? Hata: $_" -ForegroundColor Red
}

pause
