# Hýzlý Database Reset
# Drop + Update + Seed (Manuel kontrol)

Write-Host ""
Write-Host "??  DATABASE SÝLÝNECEK!" -ForegroundColor Red
Write-Host ""

$confirm = Read-Host "Devam? (E/H)"
if ($confirm -ne "E") { exit }

Write-Host ""
Write-Host "???  Database siliniyor..." -ForegroundColor Yellow
dotnet ef database drop --force

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Silindi" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "?? Yeniden oluþturuluyor..." -ForegroundColor Yellow
    dotnet ef database update
    
    if ($LASTEXITCODE -eq 0) {
   Write-Host "? Oluþturuldu" -ForegroundColor Green
        Write-Host ""
        Write-Host "?? ÞÝMDÝ YAPMANIZ GEREKENLER:" -ForegroundColor Cyan
      Write-Host ""
    Write-Host "1??  dotnet run    (Seeding için)" -ForegroundColor White
        Write-Host "2??  20 saniye bekle" -ForegroundColor White
        Write-Host "3??  Ctrl+C(Durdur)" -ForegroundColor White
        Write-Host ""
    Write-Host "? Default Users:" -ForegroundColor Cyan
        Write-Host "   admin@local / Admin123!" -ForegroundColor Gray
        Write-Host "   advisor1@local / Advisor123!" -ForegroundColor Gray
        Write-Host "   student1@local / Student123!" -ForegroundColor Gray
      Write-Host ""
    } else {
   Write-Host "? Migration hatasý!" -ForegroundColor Red
    }
} else {
    Write-Host "? Silme hatasý!" -ForegroundColor Red
}

Write-Host ""
pause
