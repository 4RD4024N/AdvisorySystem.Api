# Advisory System API

Öðrenci danýþmanlýk ve doküman yönetim sistemi için ASP.NET Core 8 Web API.

## Özellikler

### 1. Kullanýcý ve Rol Yönetimi
- **Roller**: Student, Advisor, Admin
- JWT tabanlý kimlik doðrulama
- Kayýt ve giriþ endpoint'leri

### 2. Doküman Yönetimi
- Öðrenciler doküman oluþturabilir
- Versiyonlama sistemi (dosya yükleme)
- Dosya indirme
- Doküman arama ve filtreleme

### 3. Danýþman Sistemi
- Danýþman atama
- Danýþman-öðrenci eþleþtirme
- Danýþman listesi

### 4. Yorum ve Geri Bildirim
- Doküman versiyonlarýna yorum ekleme
- Yorum silme
- Yorum listeleme

### 5. Teslim Tarihleri
- Öðrenciler için teslim tarihi yönetimi
- Durum güncelleme (Pending/Completed)
- Danýþman ve admin tarafýndan atama

### 6. Ýstatistik ve Raporlama
- Öðrenci özet istatistikleri
- Danýþman özet istatistikleri
- Admin genel bakýþ

### 7. Arama ve Filtreleme
- Doküman arama (baþlýk, tag, tarih)
- Sayfalama desteði
- Popüler tag'ler

## Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server LocalDB veya SQL Server
- Visual Studio 2022 / VS Code

### Adýmlar

1. **Projeyi klonlayýn**
```bash
git clone https://github.com/4RD4024N/AdvisorySystem.Api
cd AdvisorySystem.Api
```

2. **Paketleri geri yükleyin**
```bash
dotnet restore
```

3. **Veritabanýný oluþturun**
```bash
dotnet ef database update
```

4. **Uygulamayý çalýþtýrýn**
```bash
dotnet run
```

5. **Swagger'a eriþin**
```
https://localhost:5090/swagger
```

## API Endpoint'leri

### Kimlik Doðrulama
- `POST /api/auth/register` - Yeni kullanýcý kaydý
- `POST /api/auth/login` - Giriþ yapma (JWT token döner)

### Dokümanlar
- `GET /api/documents` - Kullanýcýnýn dokümanlarýný listele
- `POST /api/documents` - Yeni doküman oluþtur (Student)
- `POST /api/documents/{id}/versions` - Yeni versiyon yükle
- `GET /api/documents/{id}/versions` - Versiyon listesi
- `GET /api/documents/download/{versionId}` - Dosya indir

### Danýþmanlar
- `GET /api/advisors` - Tüm danýþmanlarý listele
- `POST /api/advisors/assign` - Danýþman ata (Admin/Advisor)

### Yorumlar
- `GET /api/comments/version/{versionId}` - Yorumlarý listele
- `POST /api/comments` - Yorum ekle
- `DELETE /api/comments/{id}` - Yorum sil

### Teslim Tarihleri
- `GET /api/submissions/my` - Kendi teslim tarihlerim (Student)
- `POST /api/submissions` - Yeni teslim tarihi oluþtur (Advisor/Admin)
- `PATCH /api/submissions/{id}/status` - Durum güncelle (Student)

### Ýstatistikler
- `GET /api/statistics/student/summary` - Öðrenci özeti
- `GET /api/statistics/advisor/summary` - Danýþman özeti
- `GET /api/statistics/admin/overview` - Admin genel bakýþ

### Arama
- `GET /api/search/documents` - Doküman arama (query, tags, tarih filtreleri)
- `GET /api/search/tags/popular` - Popüler tag'ler

### Debug (Geliþtirme)
- `GET /api/debug/users` - Tüm kullanýcýlarý listele
- `DELETE /api/debug/users/all` - Tüm kullanýcýlarý sil
- `GET /api/debug/seedinfo` - Seed bilgisi
- `POST /api/debug/token/{email}` - Email ile token üret

## Varsayýlan Kullanýcýlar

Uygulama baþlatýldýðýnda otomatik oluþturulur:

| Email | Þifre | Rol |
|-------|-------|-----|
| admin@local | Admin123! | Admin |
| stu@local | Arda123! | Student |

## Yapýlandýrma

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=AdvisorySystemDB;..."
  },
  "Jwt": {
    "Issuer": "AdvisorySystem",
    "Audience": "AdvisorySystem",
    "Key": "32+ karakter anahtar",
    "ExpiresMinutes": 120
  },
  "Storage": {
    "Root": "wwwroot/uploads",
    "MaxFileSize": 104857600
  }
}
```

## Swagger Kullanýmý

1. `/api/auth/login` ile token alýn
2. Sað üst "Authorize" butonuna týklayýn
3. Token'ý yapýþtýrýn (Bearer prefix olmadan)
4. Korumalý endpoint'lere eriþin

## Geliþtirme Notlarý

- Dosya boyutu limiti: 100MB (varsayýlan)
- Token geçerlilik süresi: 2 saat
- CORS: localhost:5173 (Vite frontend)
- Veritabaný: SQL Server LocalDB

## Lisans

MIT License
