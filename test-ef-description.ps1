# EF Core Description Test Script
# Bu script API'den veriyi çeker ve kontrol eder

Write-Host "?? EF Core Description Testi Baþlýyor..." -ForegroundColor Cyan
Write-Host ""

# Uygulama baþlatýlacak mý kontrol et
$process = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*AdvisorySystem*" }

if (-not $process) {
 Write-Host "??  Uygulama çalýþmýyor. Lütfen baþlatýn:" -ForegroundColor Yellow
    Write-Host "   dotnet run" -ForegroundColor White
    Write-Host ""
    exit
}

Write-Host "? Uygulama çalýþýyor" -ForegroundColor Green
Write-Host ""

# SSL sertifika doðrulamasýný atla
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

try {
    Write-Host "?? Test 1: GET /api/courses/test/raw/1" -ForegroundColor Cyan
    Write-Host ""
  
    $response = Invoke-RestMethod -Uri "https://localhost:44375/api/courses/test/raw/1" -Method Get -UseBasicParsing
    
    Write-Host "Response:" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 5
  
    Write-Host ""
    Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "?? Description Analizi:" -ForegroundColor Cyan
  Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
    
    if ($response.entity.description) {
        Write-Host "? Description VAR!" -ForegroundColor Green
        Write-Host "   Uzunluk: $($response.entity.description.Length) karakter" -ForegroundColor Gray
        Write-Host "   Önizleme: $($response.entity.description.Substring(0, [Math]::Min(80, $response.entity.description.Length)))..." -ForegroundColor Gray
    } else {
        Write-Host "? Description NULL!" -ForegroundColor Red
        Write-Host " isNull: $($response.descriptionTests.isNull)" -ForegroundColor Gray
        Write-Host "   isEmpty: $($response.descriptionTests.isEmpty)" -ForegroundColor Gray
    }
    
Write-Host ""
    Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
    
    # Test 2: Normal endpoint
    Write-Host ""
    Write-Host "?? Test 2: GET /api/courses/1" -ForegroundColor Cyan
    Write-Host ""
    
    $response2 = Invoke-RestMethod -Uri "https://localhost:44375/api/courses/1" -Method Get -UseBasicParsing
    
    if ($response2.description) {
  Write-Host "? Normal endpoint'te de Description VAR!" -ForegroundColor Green
     Write-Host "   $($response2.description)" -ForegroundColor Gray
    } else {
        Write-Host "? Normal endpoint'te Description NULL!" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "???????????????????????????????????????" -ForegroundColor Green
    Write-Host "  TEST TAMAMLANDI" -ForegroundColor Green
    Write-Host "???????????????????????????????????????" -ForegroundColor Green
    
} catch {
    Write-Host "? Hata: $_" -ForegroundColor Red
    Write-Host ""
  Write-Host "Olasý sebepler:" -ForegroundColor Yellow
    Write-Host "  - Uygulama henüz tam baþlamadý (10-15 saniye bekleyin)" -ForegroundColor Gray
    Write-Host "  - Port 44375 kullanýmda" -ForegroundColor Gray
    Write-Host "  - SSL sertifika sorunu" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Manuel test için:" -ForegroundColor Cyan
Write-Host "  https://localhost:44375/api/courses/test/raw/1" -ForegroundColor White
Write-Host "  https://localhost:44375/api/courses/diagnostics" -ForegroundColor White
Write-Host ""

pause
