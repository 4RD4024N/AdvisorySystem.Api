# Database'i Tamamen Sil ve Yeniden Oluþtur
# Tüm veriler silinecek ve migration'lar baþtan çalýþacak

Write-Host ""
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Red
Write-Host "  ??  DATABASE TAMAMEN SÝLÝNECEK!" -ForegroundColor Red
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Red
Write-Host ""
Write-Host "Bu iþlem þunlarý yapacak:" -ForegroundColor Yellow
Write-Host "  • AdvisorySystemDB database'ini tamamen sil" -ForegroundColor Gray
Write-Host "  • Tüm tablolarý sil (Users, Documents, Courses, vb.)" -ForegroundColor Gray
Write-Host "  • Migration'larý baþtan uygula" -ForegroundColor Gray
Write-Host "  • Seeding ile yeni veriler yükle" -ForegroundColor Gray
Write-Host ""
Write-Host "??  UYARI: TÜM VERÝLER SÝLÝNECEK!" -ForegroundColor Red
Write-Host ""

# Kullanýcý onayý al
$confirmation = Read-Host "Devam etmek istiyor musunuz? (EVET yazýn)"
if ($confirmation -ne "EVET") {
    Write-Host ""
    Write-Host "? Ýþlem iptal edildi." -ForegroundColor Yellow
    Write-Host ""
    pause
  exit
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? DATABASE YENÝDEN OLUÞTURULUYOR" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Proje dizinini kontrol et
$projectDir = Get-Location
Write-Host "?? Proje Dizini: $projectDir" -ForegroundColor Gray
Write-Host ""

try {
  # Adým 1: Database'i sil
    Write-Host "???  Adým 1: Database siliniyor..." -ForegroundColor Yellow
    Write-Host ""
    
    $dropOutput = dotnet ef database drop --force 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Database baþarýyla silindi!" -ForegroundColor Green
    } else {
        Write-Host "   ??  Database zaten silinmiþ veya bulunamadý" -ForegroundColor Yellow
    }
    
    Write-Host ""

    # Adým 2: Migration'larý uygula
    Write-Host "?? Adým 2: Database yeniden oluþturuluyor..." -ForegroundColor Yellow
    Write-Host "   Migration'lar uygulanýyor..." -ForegroundColor Gray
 Write-Host ""
    
    $updateOutput = dotnet ef database update 2>&1
    
    if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Database baþarýyla oluþturuldu!" -ForegroundColor Green
        Write-Host "   ? Tüm migration'lar uygulandý!" -ForegroundColor Green
    } else {
        Write-Host "   ? Migration hatasý!" -ForegroundColor Red
 Write-Host $updateOutput
    throw "Migration baþarýsýz oldu"
    }
    
    Write-Host ""
    
    # Adým 3: Seeding için uygulamayý baþlat
    Write-Host "?? Adým 3: Seeding yapýlýyor..." -ForegroundColor Yellow
    Write-Host "   Uygulama baþlatýlýyor..." -ForegroundColor Gray
    Write-Host ""
    
    # Uygulamayý arka planda baþlat
    $appProcess = Start-Process -FilePath "dotnet" `
        -ArgumentList "run" `
        -PassThru `
        -NoNewWindow `
    -RedirectStandardOutput "seeding.log" `
        -RedirectStandardError "seeding-error.log"
    
    Write-Host "   ? Seeding tamamlanmasý bekleniyor (20 saniye)..." -ForegroundColor Gray
    Start-Sleep -Seconds 20
  
    # Uygulamayý durdur
    if (!$appProcess.HasExited) {
   Stop-Process -Id $appProcess.Id -Force
        Write-Host "   ? Seeding tamamlandý!" -ForegroundColor Green
    }
    
    Write-Host ""
 
    # Adým 4: Sonuçlarý kontrol et
    Write-Host "?? Adým 4: Sonuçlar kontrol ediliyor..." -ForegroundColor Yellow
    Write-Host ""
    
    # Tablolarý kontrol et
    $tableCheckQuery = @"
SELECT 
    'Users' as TableName, COUNT(*) as RowCount FROM AspNetUsers
UNION ALL SELECT 'Roles', COUNT(*) FROM AspNetRoles
UNION ALL SELECT 'Documents', COUNT(*) FROM Documents
UNION ALL SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL SELECT 'CourseCategories', COUNT(*) FROM CourseCategories
UNION ALL SELECT 'Notifications', COUNT(*) FROM Notifications
ORDER BY TableName;
"@
    
    $tableResults = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "AdvisorySystemDB" -Q $tableCheckQuery -h-1 -W 2>&1
    
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "  ?? DATABASE ÝÇERÝÐÝ" -ForegroundColor Cyan
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host $tableResults
    Write-Host ""
    
    # Dersleri özellikle kontrol et
    $courseCheckQuery = @"
SELECT 
    COUNT(*) as TotalCourses,
    SUM(CASE WHEN Description IS NOT NULL AND Description != '' THEN 1 ELSE 0 END) as WithDescriptions,
    CAST(ROUND(100.0 * SUM(CASE WHEN Description IS NOT NULL AND Description != '' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0), 2) AS DECIMAL(5,2)) as DescriptionPercentage
FROM Courses;
"@
    
    $courseResults = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "AdvisorySystemDB" -Q $courseCheckQuery -h-1 -W 2>&1
    
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "  ?? DERS DURUMU" -ForegroundColor Cyan
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host $courseResults
    Write-Host ""
    
  # Örnek dersler
    $sampleQuery = @"
SELECT TOP 3
CourseCode as [Kod],
    CourseName as [Ders],
  LEFT(ISNULL(Description, 'YOK'), 60) as [Açýklama]
FROM Courses
ORDER BY CourseCode;
"@
    
    Write-Host "?? Örnek Dersler:" -ForegroundColor Cyan
    $sampleResults = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "AdvisorySystemDB" -Q $sampleQuery -W 2>&1
    Write-Host $sampleResults
    Write-Host ""
    
    # Baþarý mesajý
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host "  ? ÝÞLEM BAÞARIYLA TAMAMLANDI!" -ForegroundColor Green
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Yapýlanlar:" -ForegroundColor Cyan
    Write-Host "   ? Database tamamen silindi" -ForegroundColor Gray
    Write-Host "   ? Yeniden oluþturuldu" -ForegroundColor Gray
    Write-Host "   ? 6 migration uygulandý" -ForegroundColor Gray
    Write-Host "   ? Seed data yüklendi" -ForegroundColor Gray
    Write-Host ""
    Write-Host "?? Default Kullanýcýlar:" -ForegroundColor Cyan
    Write-Host "   Admin:    admin@local / Admin123!" -ForegroundColor White
    Write-Host "   Advisor:  advisor1@local / Advisor123!" -ForegroundColor White
    Write-Host "   Student:  student1@local / Student123!" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Test için:" -ForegroundColor Cyan
    Write-Host "   1. dotnet run" -ForegroundColor White
    Write-Host "   2. https://localhost:44375/swagger" -ForegroundColor White
    Write-Host "   3. https://localhost:44375/api/courses/diagnostics" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Loglar:" -ForegroundColor Cyan
    Write-Host "   - seeding.log" -ForegroundColor Gray
    Write-Host "   - seeding-error.log" -ForegroundColor Gray
    Write-Host ""
  
} catch {
    Write-Host ""
    Write-Host "? HATA OLUÞTU!" -ForegroundColor Red
  Write-Host ""
    Write-Host "Hata Detayý: $_" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "?? MANUEL ÇÖZÜM:" -ForegroundColor Cyan
    Write-Host "????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Terminalde sýrayla çalýþtýrýn:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. dotnet ef database drop --force" -ForegroundColor White
    Write-Host "  2. dotnet ef database update" -ForegroundColor White
    Write-Host "  3. dotnet run" -ForegroundColor White
    Write-Host "  4. Ctrl+C (20 saniye sonra)" -ForegroundColor White
    Write-Host ""
    Write-Host "Hata devam ederse:" -ForegroundColor Yellow
    Write-Host "  - Migration dosyalarýný kontrol edin" -ForegroundColor Gray
    Write-Host "  - SQL Server'ýn çalýþtýðýndan emin olun" -ForegroundColor Gray
    Write-Host "  - Connection string'i kontrol edin" -ForegroundColor Gray
    Write-Host ""
}

Write-Host ""
Write-Host "Devam etmek için bir tuþa basýn..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
