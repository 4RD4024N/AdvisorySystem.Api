# ?? Security Hardening Checklist - AdvisorySystem.Api

## ? Already Implemented

- [x] JWT Authentication with Bearer tokens
- [x] Rate limiting (7 different policies)
- [x] CORS configuration
- [x] Password requirements (configurable)
- [x] Role-based authorization (Admin, Advisor, Student)
- [x] File size validation middleware
- [x] HTTPS redirection

## ?? Additional Security Measures Needed

### 1. Input Validation & Sanitization

#### 1.1 Add FluentValidation

```bash
dotnet add package FluentValidation.AspNetCore
```

```csharp
// Validators/RegisterDtoValidator.cs
using FluentValidation;
using AdvisorySystem.Api.Controllers;

public class RegisterDtoValidator : AbstractValidator<AuthController.RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
      .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email too long");

        RuleFor(x => x.Password)
 .NotEmpty().WithMessage("Password is required")
     .MinimumLength(8).WithMessage("Password must be at least 8 characters")
          .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letter")
     .Matches(@"[a-z]").WithMessage("Password must contain lowercase letter")
  .Matches(@"[0-9]").WithMessage("Password must contain digit")
    .Matches(@"[\W_]").WithMessage("Password must contain special character");

        RuleFor(x => x.FullName)
  .MaximumLength(200).WithMessage("Name too long")
       .When(x => !string.IsNullOrEmpty(x.FullName));
    }
}

// Program.cs - Add validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
```

#### 1.2 SQL Injection Prevention

```csharp
// ? Already using EF Core with parameterized queries
// ? No raw SQL queries found in codebase

// If raw SQL is needed in future, use:
var courses = await _db.Courses
    .FromSqlInterpolated($"SELECT * FROM Courses WHERE Semester = {semester}")
    .ToListAsync();
// EF Core automatically parameterizes interpolated strings
```

#### 1.3 XSS Prevention

```csharp
// Middleware/XssProtectionMiddleware.cs
using System.Text.RegularExpressions;

public class XssProtectionMiddleware
{
  private readonly RequestDelegate _next;
    private static readonly Regex XssRegex = new(@"<script|javascript:|onerror=|onclick=", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public XssProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
  // Check query parameters
     foreach (var param in context.Request.Query)
        {
   if (XssRegex.IsMatch(param.Value.ToString()))
            {
    context.Response.StatusCode = 400;
           await context.Response.WriteAsJsonAsync(new { error = "Potential XSS detected" });
    return;
            }
        }

     // Enable browser XSS protection
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
  context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");

     await _next(context);
    }
}

// Program.cs
app.UseMiddleware<XssProtectionMiddleware>();
```

### 2. Secrets Management

#### 2.1 Azure Key Vault Integration

```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:Endpoint"]!);
    builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new DefaultAzureCredential());
}
```

#### 2.2 User Secrets (Development)

```bash
# Initialize user secrets
dotnet user-secrets init

# Store secrets
dotnet user-secrets set "Jwt:Key" "your-super-secret-key"
dotnet user-secrets set "ConnectionStrings:Default" "your-connection-string"
dotnet user-secrets set "Azure:StorageConnectionString" "your-azure-storage-key"
```

### 3. Content Security Policy (CSP)

```csharp
// Middleware/SecurityHeadersMiddleware.cs
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
// Content Security Policy
        context.Response.Headers.Add("Content-Security-Policy",
       "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
"connect-src 'self' https://localhost:*; " +
            "frame-ancestors 'none';");

        // Strict Transport Security (HSTS)
        context.Response.Headers.Add("Strict-Transport-Security",
      "max-age=31536000; includeSubDomains");

    // Referrer Policy
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions Policy
        context.Response.Headers.Add("Permissions-Policy",
        "geolocation=(), microphone=(), camera=()");

await _next(context);
    }
}

// Program.cs
app.UseMiddleware<SecurityHeadersMiddleware>();
```

### 4. File Upload Security

```csharp
// Middleware/FileUploadSecurityMiddleware.cs
public class FileUploadSecurityMiddleware
{
    private readonly RequestDelegate _next;
 private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".txt" };
 private static readonly string[] AllowedMimeTypes = 
    { 
        "application/pdf", 
 "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    };

    public FileUploadSecurityMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.HasFormContentType && context.Request.Form.Files.Count > 0)
        {
      foreach (var file in context.Request.Form.Files)
         {
           // Check file extension
     var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
  {
     context.Response.StatusCode = 400;
      await context.Response.WriteAsJsonAsync(new 
       { 
  error = "File type not allowed",
     allowedTypes = AllowedExtensions 
          });
                 return;
      }

           // Check MIME type
         if (!AllowedMimeTypes.Contains(file.ContentType))
       {
        context.Response.StatusCode = 400;
    await context.Response.WriteAsJsonAsync(new 
       { 
        error = "Invalid file content type" 
   });
    return;
           }

   // Check file signature (magic bytes)
          using var stream = file.OpenReadStream();
         if (!IsValidFileSignature(stream, extension))
         {
    context.Response.StatusCode = 400;
  await context.Response.WriteAsJsonAsync(new 
            { 
               error = "File signature mismatch - possible malicious file" 
                });
                  return;
       }
            }
    }

        await _next(context);
    }

 private bool IsValidFileSignature(Stream stream, string extension)
    {
        var buffer = new byte[8];
stream.Read(buffer, 0, 8);
stream.Position = 0;

  return extension switch
        {
        ".pdf" => buffer[0] == 0x25 && buffer[1] == 0x50 && 
         buffer[2] == 0x44 && buffer[3] == 0x46, // %PDF
    ".doc" => buffer[0] == 0xD0 && buffer[1] == 0xCF &&
      buffer[2] == 0x11 && buffer[3] == 0xE0, // MS Office
     ".docx" => buffer[0] == 0x50 && buffer[1] == 0x4B, // PK (ZIP)
            ".txt" => true, // Text files have no magic bytes
            _ => false
        };
    }
}

// Program.cs
app.UseMiddleware<FileUploadSecurityMiddleware>();
```

### 5. API Key Authentication (for external integrations)

```csharp
// Services/ApiKeyAuthenticationHandler.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";
 private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
    IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
     _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(ApiKeyHeaderName))
        {
 return AuthenticateResult.NoResult();
        }

        var providedApiKey = Request.Headers[ApiKeyHeaderName].ToString();
      var validApiKeys = _configuration.GetSection("ApiKeys").Get<Dictionary<string, string>>();

        if (validApiKeys == null || !validApiKeys.ContainsValue(providedApiKey))
    {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "API Client") };
   var identity = new ClaimsIdentity(claims, Scheme.Name);
     var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

// Program.cs
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
```

### 6. Audit Logging

```csharp
// Models/AuditLog.cs
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Resource { get; set; } = "";
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

// Middleware/AuditLoggingMiddleware.cs
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
 {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
      var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
 var userAgent = context.Request.Headers["User-Agent"].ToString();
   var method = context.Request.Method;
        var path = context.Request.Path;

        // Only log sensitive operations
  if (method != "GET" && !path.StartsWithSegments("/api/auth"))
        {
            var auditLog = new AuditLog
     {
         UserId = userId,
                Action = $"{method} {path}",
          Resource = path,
            IpAddress = ipAddress,
       UserAgent = userAgent,
       Success = true
 };

            try
       {
   await _next(context);

 if (context.Response.StatusCode >= 400)
     {
               auditLog.Success = false;
 auditLog.ErrorMessage = $"HTTP {context.Response.StatusCode}";
        }

     db.AuditLogs.Add(auditLog);
        await db.SaveChangesAsync();
          }
        catch (Exception ex)
            {
  auditLog.Success = false;
           auditLog.ErrorMessage = ex.Message;
     
        db.AuditLogs.Add(auditLog);
    await db.SaveChangesAsync();
             
       throw;
  }
        }
        else
 {
await _next(context);
        }
    }
}
```

### 7. Brute Force Protection Enhancement

```csharp
// Services/LoginAttemptTracker.cs
using Microsoft.Extensions.Caching.Memory;

public interface ILoginAttemptTracker
{
    Task<bool> IsLockedOut(string email);
    Task RecordFailedAttempt(string email);
    Task RecordSuccessfulAttempt(string email);
}

public class LoginAttemptTracker : ILoginAttemptTracker
{
    private readonly IMemoryCache _cache;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public LoginAttemptTracker(IMemoryCache cache)
    {
      _cache = cache;
    }

    public Task<bool> IsLockedOut(string email)
  {
        var key = $"lockout_{email}";
      return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task RecordFailedAttempt(string email)
    {
        var key = $"attempts_{email}";
        var attempts = _cache.GetOrCreate(key, entry =>
        {
      entry.SlidingExpiration = TimeSpan.FromMinutes(15);
     return 0;
        });

        attempts++;
    _cache.Set(key, attempts, TimeSpan.FromMinutes(15));

        if (attempts >= MaxAttempts)
        {
            _cache.Set($"lockout_{email}", true, LockoutDuration);
        }

        return Task.CompletedTask;
    }

    public Task RecordSuccessfulAttempt(string email)
    {
  _cache.Remove($"attempts_{email}");
   _cache.Remove($"lockout_{email}");
        return Task.CompletedTask;
    }
}

// Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ILoginAttemptTracker, LoginAttemptTracker>();

// Update AuthController.Login to use tracker
```

### 8. SQL Injection Testing

```bash
# Install sqlmap
pip install sqlmap

# Test endpoints (should all be protected by EF Core)
sqlmap -u "http://localhost:5000/api/courses?search=test" --cookie="token=YOUR_JWT_TOKEN"
sqlmap -u "http://localhost:5000/api/search/documents?query=test" --cookie="token=YOUR_JWT_TOKEN"
```

### 9. Penetration Testing Checklist

```markdown
## OWASP Top 10 Verification

- [ ] **A01: Broken Access Control**
  - Test unauthorized access to admin endpoints
  - Test horizontal privilege escalation (access other users' data)
  - Test vertical privilege escalation (student -> advisor -> admin)

- [ ] **A02: Cryptographic Failures**
  - Verify HTTPS enforcement
  - Check JWT signing algorithm (should be HS256 or RS256)
  - Verify password hashing (ASP.NET Identity uses PBKDF2)

- [ ] **A03: Injection**
  - SQL injection tests
  - XSS tests
  - Command injection tests (file operations)

- [ ] **A04: Insecure Design**
  - Review authentication flow
  - Check rate limiting effectiveness
  - Verify session management

- [ ] **A05: Security Misconfiguration**
  - Check error messages (no stack traces in production)
  - Verify CORS configuration
  - Check default credentials

- [ ] **A06: Vulnerable Components**
  - Run `dotnet list package --vulnerable`
  - Update all NuGet packages

- [ ] **A07: Identification and Authentication Failures**
  - Test password reset flow
  - Test brute force protection
  - Verify JWT expiration

- [ ] **A08: Software and Data Integrity Failures**
  - Verify file upload integrity
  - Check deserialization security

- [ ] **A09: Security Logging and Monitoring Failures**
  - Verify audit logs
  - Check Application Insights integration

- [ ] **A10: Server-Side Request Forgery (SSRF)**
  - Test file upload/download for SSRF
```

## Security Scan Tools

```bash
# 1. OWASP Dependency Check
dotnet tool install --global dependency-check
dependency-check --project "AdvisorySystem.Api" --scan "." --format "HTML"

# 2. SonarQube
dotnet tool install --global dotnet-sonarscanner
dotnet sonarscanner begin /k:"AdvisorySystem.Api"
dotnet build
dotnet sonarscanner end

# 3. Security Code Scan
dotnet add package SecurityCodeScan.VS2019

# 4. Bandit (for Python scripts if any)
pip install bandit
bandit -r .

# 5. OWASP ZAP
# Run ZAP proxy and configure browser to use it
# Browse the application to spider all endpoints
# Run active scan
```

## Deployment Security

### appsettings.Production.json

```json
{
  "Logging": {
 "LogLevel": {
      "Default": "Warning",
   "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Jwt": {
    "Key": "#{JWT_SECRET_KEY}#",
    "Issuer": "#{JWT_ISSUER}#",
    "Audience": "#{JWT_AUDIENCE}#",
    "ExpiresMinutes": 60
  },
  "ConnectionStrings": {
    "Default": "#{DB_CONNECTION_STRING}#"
  },
  "Azure": {
"ApplicationInsights": {
      "ConnectionString": "#{APPINSIGHTS_CONNECTION_STRING}#"
    },
    "StorageConnectionString": "#{AZURE_STORAGE_CONNECTION_STRING}#"
  }
}
```

### Environment Variables (Azure)

```bash
# Set via Azure Portal or CLI
az webapp config appsettings set --name your-app-name --resource-group your-rg \
  --settings \
  JWT_SECRET_KEY="your-super-secret-key" \
  DB_CONNECTION_STRING="your-connection-string" \
  APPINSIGHTS_CONNECTION_STRING="your-appinsights-key" \
  AZURE_STORAGE_CONNECTION_STRING="your-storage-key"
```

## Compliance

- [ ] **GDPR** - Implement data export/deletion endpoints
- [ ] **FERPA** (if applicable) - Student data privacy
- [ ] **SOC 2** - Audit logging, access controls
- [ ] **Privacy Policy** - Document data collection/usage
- [ ] **Terms of Service** - Legal protection

## Regular Security Tasks

```markdown
### Weekly
- [ ] Review failed login attempts
- [ ] Check rate limit violations
- [ ] Monitor suspicious file uploads

### Monthly
- [ ] Update NuGet packages
- [ ] Review audit logs
- [ ] Run vulnerability scans

### Quarterly
- [ ] Penetration testing
- [ ] Security code review
- [ ] Update security policies
```
