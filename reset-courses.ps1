# Ders Açýklamalarýný Yeniden Yükle
# Bu script mevcut dersleri siler ve yeniden seed eder

Write-Host ""
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? DERS VERÝTABANI GÜNCELLENÝYOR" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Proje dizinini kontrol et
$projectDir = Get-Location
Write-Host "?? Proje Dizini: $projectDir" -ForegroundColor Gray
Write-Host ""

# 1. SQL komutlarý
$sqlCommands = @"
USE AdvisorySystemDB;
GO
DELETE FROM StudentCourseSections;
DELETE FROM StudentCourses;
DELETE FROM CourseSchedules;
DELETE FROM Prerequisites;
DELETE FROM Courses;
DELETE FROM CourseCategories;
GO
"@

Write-Host "?? Adým 1: Mevcut dersler siliniyor..." -ForegroundColor Yellow

try {
    # SQL komutlarýný çalýþtýr
$result = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "AdvisorySystemDB" -Q $sqlCommands 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Dersler baþarýyla silindi!" -ForegroundColor Green
        Write-Host ""
        
    # 2. Uygulamayý baþlat
     Write-Host "?? Adým 2: Uygulama baþlatýlýyor (seeding için)..." -ForegroundColor Yellow
        Write-Host " Lütfen bekleyin, seeding iþlemi otomatik yapýlacak..." -ForegroundColor Gray
  Write-Host ""
     
        # Uygulamayý arka planda baþlat ve 10 saniye bekle
  $appProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -NoNewWindow -RedirectStandardOutput "seeding.log" -RedirectStandardError "seeding-error.log"
        
        Write-Host "   ? Seeding için bekleniyor (15 saniye)..." -ForegroundColor Gray
        Start-Sleep -Seconds 15
        
        # Uygulamayý durdur
        if (!$appProcess.HasExited) {
            Stop-Process -Id $appProcess.Id -Force
            Write-Host "   ? Seeding tamamlandý!" -ForegroundColor Green
        }
        
        Write-Host ""
    Write-Host "?? Adým 3: Sonuçlar kontrol ediliyor..." -ForegroundColor Yellow
        
        # Database'i kontrol et
        $checkQuery = @"
SELECT 
    COUNT(*) as TotalCourses,
    SUM(CASE WHEN Description IS NOT NULL AND Description != '' THEN 1 ELSE 0 END) as WithDescriptions,
    SUM(CASE WHEN Description IS NULL OR Description = '' THEN 1 ELSE 0 END) as WithoutDescriptions
FROM Courses;
"@
  
      $checkResult = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "AdvisorySystemDB" -Q $checkQuery -h-1 -W
  
        Write-Host ""
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
     Write-Host "  ?? DATABASE DURUMU" -ForegroundColor Cyan
        Write-Host "???????????????????????????????????????????????????" -ForegroundColor Cyan
     Write-Host $checkResult
        Write-Host ""
      
        # Örnek ders göster
   $sampleQuery = @"
SELECT TOP 3
    CourseCode,
    CourseName,
    CASE 
    WHEN Description IS NOT NULL THEN LEFT(Description, 50) + '...'
        ELSE 'NO DESCRIPTION'
    END as Description
FROM Courses
ORDER BY CourseCode;
"@
      
        Write-Host "?? Örnek Dersler:" -ForegroundColor Cyan
 $sampleResult = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "AdvisorySystemDB" -Q $sampleQuery -W
   Write-Host $sampleResult
        Write-Host ""
   
        Write-Host "???????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host "  ? ÝÞLEM TAMAMLANDI!" -ForegroundColor Green
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host ""
        Write-Host "?? Test için:" -ForegroundColor Cyan
    Write-Host "   1. Uygulamayý baþlatýn: dotnet run" -ForegroundColor White
        Write-Host "   2. Tarayýcýda açýn: https://localhost:44375/api/courses/diagnostics" -ForegroundColor White
        Write-Host ""
  Write-Host "?? Loglar:" -ForegroundColor Cyan
     Write-Host "   - seeding.log" -ForegroundColor Gray
   Write-Host "   - seeding-error.log" -ForegroundColor Gray
      
    } else {
        Write-Host "   ? SQL hatasý oluþtu!" -ForegroundColor Red
        Write-Host "   Hata kodu: $LASTEXITCODE" -ForegroundColor Red
    Write-Host ""
        Write-Host "   Alternatif: Manuel SQL çalýþtýrýn" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "? Hata: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? MANUEL ÇÖZÜM:" -ForegroundColor Cyan
  Write-Host "????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "ADIM 1: Visual Studio SQL Server Object Explorer" -ForegroundColor Yellow
  Write-Host "   - View -> SQL Server Object Explorer" -ForegroundColor Gray
    Write-Host "   - (localdb)\MSSQLLocalDB -> Databases -> AdvisorySystemDB" -ForegroundColor Gray
    Write-Host "   - Sað týk -> New Query" -ForegroundColor Gray
 Write-Host ""
    Write-Host "ADIM 2: Bu SQL'i çalýþtýrýn:" -ForegroundColor Yellow
    Write-Host ""
  Write-Host $sqlCommands -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "ADIM 3: Uygulamayý baþlatýn:" -ForegroundColor Yellow
    Write-Host "   dotnet run" -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Write-Host "Devam etmek için bir tuþa basýn..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
