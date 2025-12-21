# Advisory System API

Öğrenci danışmanlık ve doküman yönetim sistemi için ASP.NET Core 8 Web API.

## 📋 İçindekiler
- [Özellikler](#özellikler)
- [Teknoloji Stack](#teknoloji-stack)
- [Kurulum](#kurulum)
- [API Endpoint'leri](#api-endpointleri)
- [Azure Deployment](#azure-deployment)
- [Geliştirme Notları](#geliştirme-notları)

---

## Özellikler

### 1. Kullanıcı ve Rol Yönetimi
- **Roller**: Student, Advisor, Admin
- JWT tabanlı kimlik doğrulama
- Kayıt ve giriş endpoint'leri

### 2. Doküman Yönetimi
- Öğrenciler doküman oluşturabilir
- Versiyonlama sistemi (dosya yükleme)
- Dosya indirme
- Doküman arama ve filtreleme

### 3. Danışman Sistemi (v3.0 - Simplified)
- ✨ **Admin-only** öğretmen atama paneli
- Admin tüm öğrencileri görebilir (öğretmen bilgisiyle)
- Basit öğretmen atama/güncelleme/kaldırma
- Otomatik bildirimler (öğrenci + öğretmen)
- E-posta ile öğrenci arama

### 4. Yorum ve Geri Bildirim
- Doküman versiyonlarına yorum ekleme
- Yorum silme
- Yorum listeleme

### 5. Teslim Tarihleri
- Öğrenciler için teslim tarihi yönetimi
- Durum güncelleme (Pending/Completed)
- Danışman ve admin tarafından atama

### 6. İstatistik ve Raporlama
- Öğrenci özet istatistikleri
- Danışman özet istatistikleri
- Admin genel bakış

### 7. Bildirim Sistemi
- Otomatik bildirimler (yorum, danışman atama)
- Manuel bildirim gönderimi
- Toplu bildirim desteği
- Bildirim geçmişi

### 8. Öğrenci Yönetimi (Admin/Advisor)
- Öğrenci listeleme ve arama
- Öğrenci detayları görüntüleme
- Manuel bildirim gönderimi
- Toplu işlemler

### 9. Arama ve Filtreleme
- Doküman arama (başlık, tag, tarih)
- Sayfalama desteği
- Popüler tag'ler

### 10. Öğrenci Profil ve Ön Koşul Kontrolü (YENİ)
- Öğrenci profil yönetimi (öğrenci no, bölüm, GPA, tamamlanan krediler)
- Otomatik ön koşul kontrolü (MNK - Minimum Nitelik Kontrolü)
- Ders tamamlama takibi
- Proje alma yeterliliği kontrolü

### 11. Danışman Değerlendirme ve Puanlama (YENİ)
- Doküman versiyonlarına 1-100 arası puanlama
- Danışman yorumları
- Ortalama puan hesaplama
- Öğrenci performans takibi

### 12. PDF Ön İzleme (YENİ)
- Tarayıcıda doğrudan PDF görüntüleme
- İndirmeye gerek kalmadan dosya önizleme
- Dosya metadata bilgileri
- PDF.js entegrasyonu desteği

---

## 🛠️ Teknoloji Stack

### Core Framework
- **.NET 8.0** - Microsoft'un en güncel LTS framework'ü
- **C# 12.0** - Modern programlama dili özellikleri
- **ASP.NET Core Web API** - RESTful API geliştirme

### Veritabanı ve ORM
- **Microsoft SQL Server** - İlişkisel veritabanı
- **Entity Framework Core 8.0.0** - ORM (Object-Relational Mapping)
  - Code-First yaklaşımı
  - Migration desteği
  - LINQ sorgu desteği

### Güvenlik ve Kimlik Doğrulama
- **ASP.NET Core Identity** - Kullanıcı yönetimi
- **JWT (JSON Web Tokens)** - Token tabanlı kimlik doğrulama
  - HMAC-SHA256 algoritması
  - 2 saat token geçerlilik süresi
  - Role-based authorization (RBAC)

### API Dokümantasyonu
- **Swagger/OpenAPI (Swashbuckle)** - Otomatik API dokümantasyonu
  - Interactive API testing
  - JWT Bearer authentication desteği

### NuGet Paketleri

```xml
<!-- Entity Framework Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />

<!-- Identity & Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />

<!-- API Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />

<!-- Azure Services -->
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
```

### Geliştirme Araçları
- **Visual Studio 2022** - IDE
- **Visual Studio Code** - Lightweight editor
- **.NET CLI** - Command-line interface
- **Git** - Version control
- **GitHub** - Repository hosting

### Mimari Desenler
- **Repository Pattern** (EF Core DbContext)
- **Dependency Injection (DI)** - Built-in IoC container
- **Middleware Pattern** - Custom middleware (FileSizeValidation)
- **MVC Pattern** - Controller-based routing
- **Clean Architecture** - Katmanlı mimari

---

## 📦 Proje Yapısı

```
AdvisorySystem.Api/
├── Controllers/          # API endpoint'leri
│   ├── AuthController.cs
│ ├── DocumentsController.cs
│   ├── AdvisorsController.cs
│   ├── CommentsController.cs
│   ├── SubmissionsController.cs
│   ├── StatisticsController.cs
│   ├── SearchController.cs
│ ├── NotificationsController.cs
│   ├── StudentsController.cs
│   └── DebugController.cs
├── Data/# Veritabanı context
│   ├── AppDbContext.cs
│   └── IdentitySeeder.cs
├── Models/    # Veri modelleri
│   ├── AppUser.cs
│   ├── Notification.cs
│   └── ...
├── Services/     # Business logic
│ ├── IFileStorage.cs
│   ├── LocalFileStorage.cs
│   └── INotificationService.cs
├── Middleware/# Custom middleware
│   └── FileSizeValidationMiddleware.cs
├── Migrations/      # EF Core migrations
├── wwwroot/            # Static files
│   └── uploads/        # Dosya depolama
├── appsettings.json    # Yapılandırma
└── Program.cs   # Uygulama başlangıcı
```

---

## Kurulum

### Gereksinimler
- .NET 8 SDK ([İndir](https://dotnet.microsoft.com/download/dotnet/8.0))
- SQL Server LocalDB veya SQL Server
- Visual Studio 2022 / VS Code
- Git

### Adımlar

1. **Projeyi klonlayın**
```bash
git clone https://github.com/4RD4024N/AdvisorySystem.Api
cd AdvisorySystem.Api
```

2. **Paketleri geri yükleyin**
```bash
dotnet restore
```

3. **Veritabanını oluşturun**
```bash
dotnet ef database update
```

4. **Uygulamayı çalıştırın**
```bash
dotnet run
```

5. **Swagger'a erişin**
```
https://localhost:7175/swagger
```

---

## API Endpoint'leri

### Kimlik Doğrulama
- `POST /api/auth/register` - Yeni kullanıcı kaydı
- `POST /api/auth/login` - Giriş yapma (JWT token döner)

### Dokümanlar
- `GET /api/documents` - Kullanıcının dokümanlarını listele
- `POST /api/documents` - Yeni doküman oluştur (Student)
- `POST /api/documents/{id}/versions` - Yeni versiyon yükle
- `GET /api/documents/{id}/versions` - Versiyon listesi
- `GET /api/documents/download/{versionId}` - Dosya indir

### Danışmanlar (Admin Only - v3.0)
- `GET /api/advisors` - Tüm öğretmenleri listele
- `POST /api/advisors/assign` - Öğrenciye öğretmen ata/güncelle **SIMPLIFIED v3.0**
- `DELETE /api/advisors/remove/{studentId}` - Öğretmen atamasını kaldır **SIMPLIFIED v3.0**
- `GET /api/advisors/{advisorId}` - Öğretmen detayları ve öğrencileri **NEW v3.0**

### Yorumlar
- `GET /api/comments/version/{versionId}` - Yorumları listele
- `POST /api/comments` - Yorum ekle
- `DELETE /api/comments/{id}` - Yorum sil

### Bildirimler
- `GET /api/notifications` - Bildirimlerimi getir
- `GET /api/notifications/unread-count` - Okunmamış sayısı
- `PATCH /api/notifications/{id}/read` - Okundu işaretle
- `PATCH /api/notifications/mark-all-read` - Tümünü okundu işaretle

### Öğrenci Yönetimi (Admin/Advisor)
- `GET /api/students` - Tüm öğrenciler (arama desteği)
- `GET /api/students/{id}` - Öğrenci detayları
- `GET /api/students/my-students` - Öğretmenin kendi öğrencileri (Advisor) **NEW v3.0**
- `POST /api/students/{id}/send-notification` - Öğrenciye bildirim gönder
- `POST /api/students/send-bulk-notification` - Toplu bildirim
- `POST /api/students/send-notification-to-all` - Herkese bildirim
- `GET /api/students/without-advisor` - Danışmanı olmayanlar
- `GET /api/students/with-pending-submissions` - Pending teslimi olanlar

### Teslim Tarihleri
- `GET /api/submissions/my` - Kendi teslim tarihlerim (Student)
- `POST /api/submissions` - Yeni teslim tarihi oluştur (Advisor/Admin)
- `PATCH /api/submissions/{id}/status` - Durum güncelle (Student)

### İstatistikler
- `GET /api/statistics/student/summary` - Öğrenci özeti
- `GET /api/statistics/advisor/summary` - Danışman özeti
- `GET /api/statistics/admin/overview` - Admin genel bakış

### Arama
- `GET /api/search/documents` - Doküman arama (query, tags, tarih filtreleri)
- `GET /api/search/tags/popular` - Popüler tag'ler

### Debug (Geliştirme)
- `GET /api/debug/users` - Tüm kullanıcıları listele
- `DELETE /api/debug/users/all` - Tüm kullanıcıları sil
- `GET /api/debug/seedinfo` - Seed bilgisi
- `POST /api/debug/token/{email}` - Email ile token üret

**Detaylı API Dokümantasyonu:** [API_DOCUMENTATION.md](API_DOCUMENTATION.md)

---

## Varsayılan Kullanıcılar

Uygulama başlatıldığında otomatik oluşturulur:

| Email | Şifre | Rol |
|-------|-------|-----|
| admin@local | Admin123! | Admin |
| stu@local | Arda123! | Student |

---

## Yapılandırma

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=AdvisorySystemDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Issuer": "AdvisorySystem",
  "Audience": "AdvisorySystem",
    "Key": "Your-Super-Secret-Key-Minimum-32-Characters-Long-For-Security",
    "ExpiresMinutes": 120
  },
  "Storage": {
    "Root": "wwwroot/uploads",
    "MaxFileSize": 104857600
}
}
```

---

## Swagger Kullanımı

1. `/api/auth/login` ile token alın
2. Sağ üst "Authorize" butonuna tıklayın
3. Token'ı yapıştırın (Bearer prefix olmadan)
4. Korumalı endpoint'lere erişin

---

## ☁️ Azure Deployment

### Azure Servisleri (Önerilen)

#### 1. **Azure App Service**
Web API'yi barındırmak için:
```bash
# Azure CLI ile deployment
az webapp create --resource-group AdvisorySystemRG \
  --plan AdvisorySystemPlan \
  --name advisory-system-api \
  --runtime "DOTNET:8.0"

az webapp deployment source config-zip \
  --resource-group AdvisorySystemRG \
  --name advisory-system-api \
  --src publish.zip
```

**Özellikler:**
- Auto-scaling
- Custom domains
- SSL certificates (Let's Encrypt)
- Deployment slots (staging/production)
- Always On mode
- Application Insights entegrasyonu

#### 2. **Azure SQL Database**
SQL Server için:
```bash
# SQL Database oluştur
az sql server create --resource-group AdvisorySystemRG \
  --name advisorysystem-sqlserver \
  --admin-user sqladmin \
  --admin-password YourPassword123!

az sql db create --resource-group AdvisorySystemRG \
  --server advisorysystem-sqlserver \
--name AdvisorySystemDB \
  --service-objective S0
```

**Connection String (Production):**
```json
"ConnectionStrings": {
  "Default": "Server=tcp:advisorysystem-sqlserver.database.windows.net,1433;Database=AdvisorySystemDB;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;"
}
```

#### 3. **Azure Blob Storage**
Dosya depolama için (LocalFileStorage yerine):

**Paket Ekle:**
```bash
dotnet add package Azure.Storage.Blobs
```

**Implementasyon:**
```csharp
// Services/AzureBlobStorage.cs
public class AzureBlobStorage : IFileStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    
    public AzureBlobStorage(string connectionString)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
    }
    
    public async Task<(string url, long size)> SaveAsync(IFormFile file, string prefix)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("documents");
        await containerClient.CreateIfNotExistsAsync();
        
        var blobClient = containerClient.GetBlobClient($"{prefix}_{file.FileName}");
        await blobClient.UploadAsync(file.OpenReadStream(), true);
  
        return (blobClient.Uri.ToString(), file.Length);
    }
}
```

**Configuration:**
```json
"Azure": {
  "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=advisorysystemstorage;AccountKey=..."
}
```

#### 4. **Azure Key Vault**
Hassas bilgiler için (JWT Key, connection strings):

```bash
# Key Vault oluştur
az keyvault create --resource-group AdvisorySystemRG \
  --name advisorysystem-keyvault \
  --location eastus

# Secret ekle
az keyvault secret set --vault-name advisorysystem-keyvault \
  --name "JwtKey" \
  --value "Your-Super-Secret-Key"
```

**Program.cs'de kullanım:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://advisorysystem-keyvault.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

#### 5. **Application Insights**
Monitoring ve logging için:

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

**Program.cs:**
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

#### 6. **Azure CDN**
Static files ve dosya indirmeleri için:
- Blob Storage önüne CDN koy
- Global distribution
- Faster downloads

---

### Azure Deployment Checklist

**Öncesi:**
- [ ] `appsettings.Production.json` oluştur
- [ ] Connection strings'i güncelle
- [ ] JWT key'i Azure Key Vault'a taşı
- [ ] CORS ayarlarını production domain'e güncelle
- [ ] Debug endpoint'lerini production'da kapat

**Production Configuration:**
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "Default": "" // Azure SQL connection string
  },
  "Azure": {
    "StorageConnectionString": "" // Blob storage
  },
  "Logging": {
 "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  }
}
```

**Deployment Komutu:**
```bash
# Publish
dotnet publish -c Release -o ./publish

# Zip oluştur
cd publish
tar -czf ../publish.zip *
cd ..

# Azure'a deploy
az webapp deployment source config-zip \
  --resource-group AdvisorySystemRG \
  --name advisory-system-api \
  --src publish.zip
```

**Alternatif: GitHub Actions**
```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
  - name: Setup .NET
  uses: actions/setup-dotnet@v1
with:
        dotnet-version: '8.0.x'
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
  - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: advisory-system-api
    publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
   package: ./publish
```

---

### Azure Maliyet Tahmini (Aylık)

| Servis | Tier | Tahmini Maliyet |
|--------|------|-----------------|
| App Service | B1 (Basic) | $13/ay |
| Azure SQL Database | S0 (10 DTU) | $15/ay |
| Blob Storage | Standard | $1-5/ay |
| Application Insights | Temel | Ücretsiz (5GB/ay) |
| **Toplam** | | **~$30-35/ay** |

**Öğrenci Hesabı ile:**
- $100 ücretsiz kredi
- İlk 12 ay birçok servis ücretsiz
- [Azure for Students](https://azure.microsoft.com/free/students/)

---

### Alternative Deployment Options

#### Docker + Azure Container Instances
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AdvisorySystem.Api.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdvisorySystem.Api.dll"]
```

```bash
# Build ve push
docker build -t advisorysystem-api .
docker tag advisorysystem-api youracr.azurecr.io/advisorysystem-api
docker push youracr.azurecr.io/advisorysystem-api

# Azure Container Instance'a deploy
az container create --resource-group AdvisorySystemRG \
  --name advisorysystem-api \
  --image youracr.azurecr.io/advisorysystem-api \
  --cpu 1 --memory 1 \
  --ports 80
```

---

## Geliştirme Notları

### Performans
- Dosya boyutu limiti: 100MB (varsayılan)
- Token geçerlilik süresi: 2 saat
- Async/Await pattern kullanımı
- LINQ deferred execution
- Database indexing

### Güvenlik
- JWT token validation
- Role-based authorization
- Password hashing (PBKDF2)
- CORS policy
- HTTPS enforcement
- File size validation middleware

### CORS
- Development: `localhost:5173` (Vite frontend)
- Production: Domain'i `appsettings.Production.json`'da güncelle

### Veritabanı
- SQL Server LocalDB (development)
- Azure SQL Database (production)
- Entity Framework Core migrations
- Seed data (admin, student)

### Logging
- Development: Console logging
- Production: Application Insights
- Structured logging
- Error tracking

---

## 🚀 Gelecek Geliştirmeler

- [ ] Email notifications (SMTP)
- [ ] Background services (deadline reminders)
- [ ] Real-time features (SignalR)
- [ ] PDF generation (reports)
- [ ] Advanced analytics
- [ ] Mobile app (Xamarin/MAUI)
- [ ] Audit logging
- [ ] Two-factor authentication (2FA)
- [ ] Document templates
- [ ] Bulk operations

---

## 📚 Dokümantasyon

- **Technology Stack:** [TECHNOLOGY_STACK.md](TECHNOLOGY_STACK.md)
- **Presentation Summary:** [PRESENTATION_SUMMARY.md](PRESENTATION_SUMMARY.md)
- **Use Cases & Requirements:** [USE_CASE_SUMMARY.md](USE_CASE_SUMMARY.md)
- **API Guide:** [API_DOCUMENTATION.md](API_DOCUMENTATION.md)
- ✨ **Admin Advisor Management (v3.0):** [ADMIN_ADVISOR_MANAGEMENT_API.md](ADMIN_ADVISOR_MANAGEMENT_API.md) **YENİ - SIMPLIFIED**
- **Students API:** [STUDENTS_API_GUIDE.md](STUDENTS_API_GUIDE.md)
- **Token Refresh:** [TOKEN_REFRESH_QUICK_GUIDE.md](TOKEN_REFRESH_QUICK_GUIDE.md)
- **ER Diagram:** [ER_DIAGRAM.html](ER_DIAGRAM.html) **v2.1**
- **Swagger:** `https://localhost:7175/swagger`

### Archived (Old Versions)
- ~~Advisor API (v2.1)~~ → Replaced by v3.0
- ~~Advisor Assignment Guide~~ → Replaced by v3.0
- ~~Advisor Assignment Summary~~ → Replaced by v3.0

---

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit yapın (`git commit -m 'Add some AmazingFeature'`)
4. Push yapın (`git push origin feature/AmazingFeature`)
5. Pull Request açın

---

## 📞 İletişim

- **Repository:** https://github.com/4RD4024N/AdvisorySystem.Api
- **Issues:** [GitHub Issues](https://github.com/4RD4024N/AdvisorySystem.Api/issues)

---

## 📄 Lisans

MIT License - Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

## 🙏 Teşekkürler

- Microsoft .NET Team
- ASP.NET Core Community
- Entity Framework Core Contributors
- Swagger/OpenAPI Maintainers

---

**Proje Durumu:** 🟢 Active Development  
**Son Güncelleme:** 2024-12-20  
**Versiyon:** 3.0.0

**🆕 v3.0.0 Yeni Özellikler (Simplified):**
- ✅ **Admin-only** öğretmen atama sistemi
- ✅ Basitleştirilmiş API (4 ana endpoint)
- ✅ Tam öğrenci listesi (advisor bilgisiyle)
- ✅ Tek endpoint ile atama/güncelleme
- ✅ Hazır admin UI örneği (HTML/CSS/JS)
- ✅ Otomatik bildirimler (update durumunda 3 taraf)
