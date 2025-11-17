# ??? Advisory System - Teknoloji ve Araçlar Raporu

## ?? Genel Bakýþ

**Proje Adý:** Advisory System API  
**Platform:** .NET 8.0  
**Mimari:** RESTful Web API  
**Veritabaný:** Microsoft SQL Server  
**Geliþtirme Dili:** C# 12.0

---

## ?? Kullanýlan Teknolojiler

### 1. Core Framework ve Dil

#### .NET 8.0 (LTS)
**Nedir:** Microsoft'un en güncel Long-Term Support framework'ü  
**Neden Seçildi:**
- ? 3 yýl destek garantisi (2024-2027)
- ? En iyi performans (Native AOT, improved GC)
- ? Cloud-native optimizasyonlar
- ? Modern C# 12 özellikleri

**Özellikler:**
- Cross-platform (Windows, Linux, macOS)
- Minimal API desteði
- Built-in dependency injection
- Configuration management
- Middleware pipeline

#### C# 12.0
**Nedir:** Modern, type-safe programlama dili  
**Kullanýlan Özellikler:**
- Primary constructors
- Collection expressions
- Ref readonly parameters
- Default lambda parameters
- Alias any type
- Inline arrays

**Örnek Kullaným:**
```csharp
// Primary Constructor
public class Document(string title, string ownerId)
{
    public string Title { get; } = title;
    public string OwnerId { get; } = ownerId;
}

// Record types
public record CreateSubmissionDto(string StudentId, DateTime DueDate);
```

---

### 2. Web Framework

#### ASP.NET Core 8.0 Web API
**Nedir:** RESTful API geliþtirme framework'ü  
**Kullaným Alanlarý:**
- HTTP endpoint'leri (Controller-based)
- Routing ve model binding
- Content negotiation (JSON)
- CORS policy management
- Exception handling

**Controller Örneði:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
  [HttpGet]
    public async Task<IActionResult> GetMyDocuments()
    {
        // Implementation
    }
}
```

**Özellikler:**
- Attribute routing
- Model validation
- Action filters
- Middleware pipeline
- Built-in DI container

---

### 3. Veritabaný Teknolojileri

#### Microsoft SQL Server
**Nedir:** Enterprise-grade iliþkisel veritabaný  
**Versiyon:** SQL Server 2019/2022 / Azure SQL Database  
**Kullaným:**
- Development: SQL Server LocalDB
- Production: Azure SQL Database (önerilen)

**Özellikler:**
- ACID transactions
- Referential integrity
- Full-text search
- Backup and recovery
- Query optimization

**Tablolar:**
```
AspNetUsers          (Identity kullanýcýlarý)
AspNetRoles (Roller: Student, Advisor, Admin)
Documents          (Doküman kayýtlarý)
DocumentVersions     (Dosya versiyonlarý)
Comments             (Yorumlar)
Submissions      (Teslim tarihleri)
Notifications  (Bildirimler)
```

#### Entity Framework Core 8.0
**Nedir:** Modern ORM (Object-Relational Mapping)  
**Kullaným Amacý:** Database operations, migrations, LINQ queries

**Özellikler:**
- Code-First yaklaþým
- Automatic migrations
- LINQ to SQL
- Change tracking
- Lazy/Eager loading

**Migration Örneði:**
```bash
# Migration oluþtur
dotnet ef migrations add InitialCreate

# Database güncelle
dotnet ef database update

# Migration geri al
dotnet ef database update PreviousMigration
```

**LINQ Örneði:**
```csharp
var documents = await _db.Documents
    .Where(d => d.OwnerUserId == userId)
    .Include(d => d.Versions)
    .OrderByDescending(d => d.CreatedAt)
    .ToListAsync();
```

---

### 4. Güvenlik ve Kimlik Doðrulama

#### ASP.NET Core Identity
**Nedir:** Kullanýcý ve rol yönetim sistemi  
**Özellikler:**
- User management (CRUD)
- Password hashing (PBKDF2)
- Role-based authorization
- Claims-based identity
- Account lockout

**Kullaným:**
```csharp
// User oluþturma
var user = new AppUser { UserName = email, Email = email };
await _userManager.CreateAsync(user, password);

// Role atama
await _userManager.AddToRoleAsync(user, "Student");

// Role kontrolü
var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
```

#### JWT (JSON Web Tokens)
**Nedir:** Stateless authentication token standardý  
**RFC:** 7519  
**Algoritma:** HMAC-SHA256

**Token Yapýsý:**
```
Header.Payload.Signature

Örnek:
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiJ1c2VyLWlkIiwiZW1haWwiOiJ1c2VyQGV4YW1wbGUuY29tIiwicm9sZSI6IlN0dWRlbnQifQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**Claims (Payload):**
```json
{
  "sub": "user-id-123",
  "email": "student@university.edu",
  "name": "student@university.edu",
  "role": "Student",
  "nameidentifier": "user-id-123",
  "uid": "user-id-123",
  "jti": "unique-token-id",
  "exp": 1736175600
}
```

**Configuration:**
```json
"Jwt": {
  "Issuer": "AdvisorySystem",
  "Audience": "AdvisorySystem",
  "Key": "Your-Super-Secret-Key-32-Chars-Min",
  "ExpiresMinutes": 1440
}
```

---

### 5. API Dokümantasyon

#### Swagger / OpenAPI 3.0
**Paket:** Swashbuckle.AspNetCore 6.6.2  
**Nedir:** Otomatik API dokümantasyon aracý

**Özellikler:**
- Interactive API testing
- Auto-generated documentation
- Request/Response schemas
- JWT Bearer authentication UI
- Export to OpenAPI JSON/YAML

**Eriþim:**
```
Development: https://localhost:7175/swagger
Production: /swagger (optional, genelde kapalý)
```

**Configuration:**
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Advisory System API", 
Version = "v1" 
    });
    
    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
     Type = SecuritySchemeType.Http,
        Scheme = "bearer",
    BearerFormat = "JWT"
    });
});
```

---

### 6. Dosya Depolama

#### Local File Storage (Development)
**Implementasyon:** `LocalFileStorage.cs`  
**Path:** `wwwroot/uploads`  
**Kullaným:** Development ve test ortamý

```csharp
public class LocalFileStorage : IFileStorage
{
    public async Task<(string url, long size)> SaveAsync(IFormFile file, string prefix)
    {
      var uploadsFolder = Path.Combine(_webRoot, "uploads");
 var fileName = $"{prefix}_{file.FileName}";
     var filePath = Path.Combine(uploadsFolder, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
 
        return ($"/uploads/{fileName}", file.Length);
    }
}
```

#### Azure Blob Storage (Production - Önerilen)
**Paket:** Azure.Storage.Blobs 12.19.1  
**Kullaným:** Production ortamý, ölçeklenebilir dosya depolama

**Özellikler:**
- Unlimited storage
- CDN integration
- Geo-redundancy
- SAS tokens (secure access)
- Lifecycle management

**Implementasyon:**
```csharp
public class AzureBlobStorage : IFileStorage
{
    private readonly BlobServiceClient _blobClient;
    
    public async Task<(string url, long size)> SaveAsync(IFormFile file, string prefix)
    {
     var container = _blobClient.GetBlobContainerClient("documents");
     var blob = container.GetBlobClient($"{prefix}_{file.FileName}");
        
        await blob.UploadAsync(file.OpenReadStream(), true);
        return (blob.Uri.ToString(), file.Length);
    }
}
```

---

### 7. Dependency Injection

#### Built-in IoC Container
**Nedir:** Inversion of Control container  
**Kullaným:** Service registration ve lifetime management

**Lifetime Types:**
- **Singleton:** Uygulama boyunca tek instance
- **Scoped:** HTTP request baþýna bir instance
- **Transient:** Her inject edildiðinde yeni instance

```csharp
// Program.cs
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
builder.Services.AddTransient<IEmailService, EmailService>();
```

**Constructor Injection:**
```csharp
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFileStorage _fileStorage;
    
    public DocumentsController(AppDbContext db, IFileStorage fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }
}
```

---

### 8. Configuration Management

#### appsettings.json
**Nedir:** Uygulama yapýlandýrma dosyasý  
**Kullaným:** Connection strings, API keys, settings

**Yapý:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=...;Database=..."
  },
  "Jwt": {
    "Issuer": "AdvisorySystem",
    "Key": "secret-key",
    "ExpiresMinutes": 1440
  },
  "Storage": {
    "Root": "wwwroot/uploads",
    "MaxFileSize": 104857600
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
}
}
```

**Environment-specific:**
- `appsettings.json` (base)
- `appsettings.Development.json`
- `appsettings.Production.json`

**Kullaným:**
```csharp
var jwtKey = builder.Configuration["Jwt:Key"];
var connectionString = builder.Configuration.GetConnectionString("Default");
```

---

### 9. Middleware

#### Built-in Middleware
```csharp
app.UseHttpsRedirection();      // HTTP ? HTTPS redirect
app.UseCors("AllowFrontend");   // CORS policy
app.UseAuthentication();        // JWT validation
app.UseAuthorization();         // Role-based access
app.UseStaticFiles();   // wwwroot files
```

#### Custom Middleware
**FileSizeValidationMiddleware:**
```csharp
public class FileSizeValidationMiddleware
{
    private readonly long _maxFileSize = 100 * 1024 * 1024; // 100MB
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
  if (context.Request.HasFormContentType)
   {
            var files = context.Request.Form.Files;
   if (files.Any(f => f.Length > _maxFileSize))
   {
  context.Response.StatusCode = 413;
         await context.Response.WriteAsync("File too large");
    return;
    }
        }
   await next(context);
    }
}
```

---

### 10. Logging

#### Built-in Logging
**Provider'lar:**
- Console (Development)
- Debug
- EventSource
- Application Insights (Production)

**Kullaným:**
```csharp
public class DocumentsController : ControllerBase
{
    private readonly ILogger<DocumentsController> _logger;
    
    public DocumentsController(ILogger<DocumentsController> logger)
    {
    _logger = logger;
    }
    
 public async Task<IActionResult> CreateDocument(CreateDocumentDto dto)
    {
        _logger.LogInformation("Creating document: {Title}", dto.Title);
        
try
        {
            // Implementation
        }
      catch (Exception ex)
        {
     _logger.LogError(ex, "Failed to create document");
       return StatusCode(500);
        }
    }
}
```

**Log Levels:**
- Trace (0)
- Debug (1)
- Information (2)
- Warning (3)
- Error (4)
- Critical (5)

---

### 11. Testing (Future)

#### xUnit
**Nedir:** .NET test framework'ü  
**Kullaným:** Unit testing

```csharp
public class DocumentServiceTests
{
    [Fact]
    public async Task CreateDocument_ValidData_ReturnsDocument()
    {
    // Arrange
        var service = new DocumentService();
 
  // Act
     var result = await service.CreateAsync("Test Title", "user-id");
        
   // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Title", result.Title);
    }
}
```

#### Moq
**Nedir:** Mocking framework  
**Kullaným:** Dependency mocking

```csharp
var mockRepo = new Mock<IDocumentRepository>();
mockRepo.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new Document { Id = 1, Title = "Test" });
```

---

## ?? Geliþtirme Araçlarý

### 1. IDE ve Editor

#### Visual Studio 2022 Community/Professional
**Özellikler:**
- IntelliSense (code completion)
- Integrated debugger
- NuGet Package Manager
- Database tools (SQL Server Object Explorer)
- Git integration
- Built-in terminal

#### Visual Studio Code
**Extensions:**
- C# Dev Kit
- .NET Extension Pack
- REST Client
- GitLens

---

### 2. Version Control

#### Git
**Kullaným:**
```bash
git add .
git commit -m "feat: Add document upload feature"
git push origin master
```

#### GitHub
**Repository:** https://github.com/4RD4024N/AdvisorySystem.Api

**Özellikler:**
- Code hosting
- Issue tracking
- Pull requests
- GitHub Actions (CI/CD)
- Wiki documentation

---

### 3. Package Management

#### NuGet
**Önemli Paketler:**

| Paket | Versiyon | Kullaným |
|-------|----------|----------|
| Microsoft.EntityFrameworkCore | 8.0.0 | ORM |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server provider |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.0 | Identity |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT auth |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger |
| Azure.Storage.Blobs | 12.19.1 | Blob storage |

**Komutlar:**
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet restore
dotnet list package
```

---

### 4. CLI Tools

#### .NET CLI
```bash
# Project
dotnet new webapi -n AdvisorySystem.Api
dotnet build
dotnet run
dotnet publish -c Release

# Database
dotnet ef migrations add InitialCreate
dotnet ef database update

# Testing
dotnet test

# Package
dotnet add package PackageName
dotnet restore
```

---

### 5. API Testing

#### Swagger UI
**URL:** https://localhost:7175/swagger  
**Özellikler:**
- Interactive testing
- Request/Response preview
- Schema documentation
- JWT authentication

#### Postman
**Kullaným:**
- Collection management
- Environment variables
- Automated tests
- Request history

#### cURL
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin123!"}'
```

---

## ?? Cloud ve Deployment

### 1. Azure Services

#### Azure App Service
**Nedir:** PaaS web hosting  
**Özellikler:**
- Auto-scaling
- Custom domains
- SSL certificates
- Deployment slots
- Always On

#### Azure SQL Database
**Nedir:** Managed SQL Server  
**Özellikler:**
- Automatic backups
- Point-in-time restore
- Geo-replication
- Elastic pools

#### Azure Blob Storage
**Nedir:** Object storage service  
**Kullaným:** File uploads (documents, images)

#### Azure Key Vault
**Nedir:** Secret management  
**Kullaným:** API keys, connection strings, JWT keys

#### Application Insights
**Nedir:** APM (Application Performance Monitoring)  
**Özellikler:**
- Performance monitoring
- Error tracking
- Usage analytics
- Custom metrics

---

### 2. CI/CD

#### GitHub Actions
**Workflow Örneði:**
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ master ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - uses: actions/setup-dotnet@v1
      with:
 dotnet-version: '8.0.x'
  - run: dotnet build
    - run: dotnet publish -c Release
    - uses: azure/webapps-deploy@v2
```

---

## ?? Mimari Desenler

### 1. Repository Pattern
**EF Core DbContext acts as repository**
```csharp
public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }
}
```

### 2. Dependency Injection
**Constructor injection everywhere**

### 3. MVC Pattern
**Controller-based routing**

### 4. Middleware Pipeline
**Request/Response processing**

### 5. Clean Architecture
**Separation of concerns**
- Controllers: Presentation
- Services: Business logic
- Data: Persistence
- Models: Entities

---

## ?? Güvenlik Mekanizmalarý

### 1. Authentication
- JWT Bearer tokens
- HMAC-SHA256 signing
- 24-hour expiration
- Token refresh mechanism

### 2. Authorization
- Role-based (RBAC)
- Attribute-based ([Authorize(Roles="Admin")])
- Claims-based

### 3. Data Protection
- Password hashing (PBKDF2)
- HTTPS enforcement
- CORS policy
- SQL injection prevention (parameterized queries)

### 4. File Upload Security
- Size validation (100MB max)
- Type validation
- Middleware protection

---

## ?? Performans Optimizasyonlarý

### 1. Database
- Indexing (DocumentVersion: DocumentId, VersionNo)
- Async operations
- Connection pooling
- Pagination (20 records/page)

### 2. API
- Async/await pattern
- LINQ deferred execution
- Minimal data transfer
- Response compression

### 3. Caching (Future)
- Memory cache
- Distributed cache (Redis)
- Response caching

---

## ?? Dokümantasyon Araçlarý

### 1. Swagger/OpenAPI
- Auto-generated API docs
- Interactive testing UI

### 2. Markdown
- README.md
- API_DOCUMENTATION.md
- USE_CASE_SUMMARY.md

### 3. XML Comments
```csharp
/// <summary>
/// Creates a new document
/// </summary>
/// <param name="dto">Document creation data</param>
/// <returns>Created document ID</returns>
[HttpPost]
public async Task<IActionResult> Create(CreateDocumentDto dto)
```

---

## ?? Toplam Teknoloji Sayýsý

| Kategori | Teknoloji Sayýsý |
|----------|------------------|
| **Framework & Dil** | 2 (.NET 8, C# 12) |
| **Web** | 1 (ASP.NET Core) |
| **Database** | 2 (SQL Server, EF Core) |
| **Güvenlik** | 2 (Identity, JWT) |
| **Storage** | 2 (Local, Azure Blob) |
| **Dokümantasyon** | 1 (Swagger) |
| **Cloud** | 5 (Azure services) |
| **IDE** | 2 (VS 2022, VS Code) |
| **Testing** | 2 (xUnit, Moq) |
| **CI/CD** | 1 (GitHub Actions) |
| **TOPLAM** | **20+** teknoloji |

---

## ?? Teknoloji Seçim Kriterleri

### 1. .NET 8 Seçimi
? Microsoft desteði (LTS)  
? Performans iyileþtirmeleri  
? Modern C# özellikleri  
? Cloud-native optimizasyonlar  
? Cross-platform  

### 2. SQL Server Seçimi
? Enterprise-grade güvenilirlik  
? ACID transactions
? Azure entegrasyonu  
? Rich query capabilities  
? Tooling support  

### 3. JWT Seçimi
? Stateless authentication  
? Scalability (no session storage)  
? Industry standard (RFC 7519)  
? Cross-platform compatibility  
? Mobile app support  

---

## ?? Öðrenme Kaynaklarý

### Resmi Dokümantasyon
- [.NET 8 Docs](https://docs.microsoft.com/dotnet/core)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [EF Core Docs](https://docs.microsoft.com/ef/core)
- [Azure Docs](https://docs.microsoft.com/azure)

### Topluluk Kaynaklarý
- [StackOverflow](https://stackoverflow.com/questions/tagged/.net)
- [GitHub - .NET](https://github.com/dotnet)
- [Dev.to - .NET](https://dev.to/t/dotnet)

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-06  
**Versiyon:** 1.0.0  
**Amaç:** Sunum ve Eðitim
