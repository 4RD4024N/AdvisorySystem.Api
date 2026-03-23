# Rate Limiting Implementation Plan for AdvisorySystem.Api

## Overview
This document outlines the rate limiting strategy for all API endpoints to prevent abuse, ensure fair usage, and protect system resources.

---

## Technology Choice: .NET 8 Built-in Rate Limiting

Using `Microsoft.AspNetCore.RateLimiting` (built-in .NET 8) for the following reasons:
- Native support, no external dependencies
- Flexible policy configuration
- Redis support for distributed scenarios
- Easy integration with middleware pipeline

---

## Rate Limiting Policies

### 1. **Authentication Endpoints** (Most Restrictive)
| Endpoint | Policy | Limit | Window | Reason |
|----------|--------|-------|--------|--------|
| `POST /api/auth/login` | `auth-strict` | 5 requests | 1 minute | Prevent brute force attacks |
| `POST /api/auth/register` | `auth-strict` | 3 requests | 1 minute | Prevent spam registrations |
| `POST /api/auth/refresh` | `auth-relaxed` | 20 requests | 1 minute | Token refresh needs flexibility |
| `GET /api/auth/validate` | `auth-relaxed` | 30 requests | 1 minute | Validation can be frequent |

### 2. **File Upload Endpoints** (Restrictive)
| Endpoint | Policy | Limit | Window | Reason |
|----------|--------|-------|--------|--------|
| `POST /api/documents/{id}/versions` | `upload` | 10 requests | 1 minute | Prevent storage abuse |
| `POST /api/storage/upload` | `upload` | 10 requests | 1 minute | Storage protection |

### 3. **File Download/Preview Endpoints** (Moderate)
| Endpoint | Policy | Limit | Window | Reason |
|----------|--------|-------|--------|--------|
| `GET /api/documents/download/{id}` | `download` | 30 requests | 1 minute | Allow reasonable downloads |
| `GET /api/documents/preview/{id}` | `download` | 50 requests | 1 minute | Preview can be more frequent |

### 4. **Search Endpoints** (Moderate - Resource Intensive)
| Endpoint | Policy | Limit | Window | Reason |
|----------|--------|-------|--------|--------|
| `GET /api/search/documents` | `search` | 30 requests | 1 minute | DB query protection |
| `GET /api/search/tags/popular` | `search` | 30 requests | 1 minute | Aggregation queries |

### 5. **CRUD Operations** (Standard)
| Endpoint | Policy | Limit | Window | Reason |
|----------|--------|-------|--------|--------|
| `GET /api/documents` | `standard` | 60 requests | 1 minute | Normal read operations |
| `POST /api/documents` | `standard` | 30 requests | 1 minute | Create operations |
| `GET /api/courses/*` | `standard` | 100 requests | 1 minute | Course data is frequently accessed |
| `GET /api/notifications` | `standard` | 60 requests | 1 minute | Polling support |

### 6. **Admin/Statistics Endpoints** (Relaxed for admins)
| Endpoint | Policy | Limit | Window | Reason |
|----------|--------|-------|--------|--------|
| `GET /api/statistics/*` | `admin` | 100 requests | 1 minute | Admin dashboards need data |
| `GET /api/diagnostics/*` | `admin` | 50 requests | 1 minute | Debugging purposes |

---

## Implementation Steps

### Step 1: Add Rate Limiting Services in `Program.cs`

```csharp
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

// Add rate limiting services
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Global limiter - fallback for all endpoints
  options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
  RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
  factory: partition => new FixedWindowRateLimiterOptions
  {
    AutoReplenishment = true,
PermitLimit = 100,
       Window = TimeSpan.FromMinutes(1)
            }));

    // Auth-strict policy (login, register)
    options.AddFixedWindowLimiter("auth-strict", opt =>
    {
    opt.PermitLimit = 5;
   opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    
    // Auth-relaxed policy (refresh, validate)
    options.AddFixedWindowLimiter("auth-relaxed", opt =>
    {
     opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Upload policy
options.AddFixedWindowLimiter("upload", opt =>
    {
      opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
      opt.QueueLimit = 2;
    });

    // Download policy
 options.AddFixedWindowLimiter("download", opt =>
    {
        opt.PermitLimit = 50;
    opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Search policy
    options.AddSlidingWindowLimiter("search", opt =>
    {
  opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6; // 10-second segments
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
  opt.QueueLimit = 3;
    });

    // Standard CRUD policy
    options.AddFixedWindowLimiter("standard", opt =>
    {
        opt.PermitLimit = 60;
    opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Admin policy (more relaxed)
    options.AddFixedWindowLimiter("admin", opt =>
    {
 opt.PermitLimit = 100;
opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    // Custom response for rate limit exceeded
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
   
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
  await context.HttpContext.Response.WriteAsJsonAsync(new
       {
   error = "Too many requests",
        message = "Rate limit exceeded. Please try again later.",
 retryAfter = retryAfter.TotalSeconds
            }, token);
        }
        else
        {
       await context.HttpContext.Response.WriteAsJsonAsync(new
            {
        error = "Too many requests",
        message = "Rate limit exceeded. Please try again later."
       }, token);
    }
    };
});
```

### Step 2: Add Rate Limiting Middleware in Pipeline

```csharp
// Add AFTER UseRouting, BEFORE UseAuthentication
app.UseRateLimiter();
```

### Step 3: Apply Policies to Controllers

#### AuthController.cs
```csharp
[HttpPost("login")]
[AllowAnonymous]
[EnableRateLimiting("auth-strict")]
public async Task<IActionResult> Login(LoginDto dto) { ... }

[HttpPost("register")]
[AllowAnonymous]
[EnableRateLimiting("auth-strict")]
public async Task<IActionResult> Register(RegisterDto dto) { ... }

[HttpPost("refresh")]
[Authorize]
[EnableRateLimiting("auth-relaxed")]
public async Task<IActionResult> RefreshToken() { ... }
```

#### DocumentsController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("standard")] // Default for controller
public class DocumentsController : ControllerBase
{
    [HttpPost("{id:int}/versions")]
    [EnableRateLimiting("upload")] // Override for upload
    public async Task<IActionResult> Upload(...) { ... }

  [HttpGet("download/{versionId:int}")]
    [EnableRateLimiting("download")]
 public async Task<IActionResult> Download(...) { ... }

    [HttpGet("preview/{versionId:int}")]
    [EnableRateLimiting("download")]
    public async Task<IActionResult> PreviewPdf(...) { ... }
}
```

#### SearchController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("search")]
public class SearchController : ControllerBase { ... }
```

#### StatisticsController.cs & DiagnosticsController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("admin")]
public class StatisticsController : ControllerBase { ... }
```

---

## Endpoint-Policy Mapping Summary

| Controller | Default Policy | Special Overrides |
|------------|---------------|-------------------|
| `AuthController` | - | `auth-strict` (login, register), `auth-relaxed` (refresh, validate) |
| `DocumentsController` | `standard` | `upload` (versions), `download` (download, preview) |
| `SearchController` | `search` | - |
| `SubmissionsController` | `standard` | - |
| `CoursesController` | `standard` | - |
| `CourseSelectionController` | `standard` | - |
| `ScheduleController` | `standard` | - |
| `NotificationsController` | `standard` | - |
| `StudentsController` | `standard` | - |
| `AdvisorsController` | `standard` | - |
| `RatingsController` | `standard` | - |
| `CommentsController` | `standard` | - |
| `StatisticsController` | `admin` | - |
| `DiagnosticsController` | `admin` | - |
| `StorageController` | `standard` | `upload` (upload endpoints) |

---

## Required NuGet Packages

No additional packages needed - `Microsoft.AspNetCore.RateLimiting` is built into .NET 8.

---

## Configuration Options (appsettings.json)

```json
{
  "RateLimiting": {
    "EnableGlobalLimiter": true,
    "GlobalLimit": 100,
    "GlobalWindowMinutes": 1,
    "Policies": {
  "AuthStrict": { "PermitLimit": 5, "WindowMinutes": 1 },
      "AuthRelaxed": { "PermitLimit": 30, "WindowMinutes": 1 },
      "Upload": { "PermitLimit": 10, "WindowMinutes": 1 },
      "Download": { "PermitLimit": 50, "WindowMinutes": 1 },
      "Search": { "PermitLimit": 30, "WindowMinutes": 1 },
      "Standard": { "PermitLimit": 60, "WindowMinutes": 1 },
      "Admin": { "PermitLimit": 100, "WindowMinutes": 1 }
    }
  }
}
```

---

## Testing Plan

### Manual Testing
1. Use tools like **Postman**, **curl**, or **hey** to send rapid requests
2. Verify 429 responses after limit exceeded
3. Check `Retry-After` header in response

### Automated Testing
```csharp
[Fact]
public async Task Login_ExceedsRateLimit_Returns429()
{
    // Send 6 requests rapidly
    for (int i = 0; i < 6; i++)
    {
        var response = await _client.PostAsync("/api/auth/login", content);
    if (i >= 5)
        {
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
    }
}
```

---

## Monitoring & Logging

Add logging for rate limit events:

```csharp
options.OnRejected = async (context, token) =>
{
    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogWarning(
    "Rate limit exceeded for {Endpoint} by {User} from {IP}",
        context.HttpContext.Request.Path,
    context.HttpContext.User.Identity?.Name ?? "anonymous",
        context.HttpContext.Connection.RemoteIpAddress);
    
    // ... response handling
};
```

---

## Future Enhancements

1. **Redis-based distributed rate limiting** for load-balanced scenarios
2. **User-tier based limits** (e.g., premium users get higher limits)
3. **IP-based limiting** for anonymous endpoints
4. **Rate limit headers** (`X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`)
5. **Dashboard integration** for real-time monitoring

---

## Implementation Checklist

- [ ] Add rate limiting services in `Program.cs`
- [ ] Add `app.UseRateLimiter()` middleware
- [ ] Apply `[EnableRateLimiting]` to `AuthController`
- [ ] Apply `[EnableRateLimiting]` to `DocumentsController`
- [ ] Apply `[EnableRateLimiting]` to `SearchController`
- [ ] Apply `[EnableRateLimiting]` to `SubmissionsController`
- [ ] Apply `[EnableRateLimiting]` to `StorageController`
- [ ] Apply `[EnableRateLimiting]` to `StatisticsController`
- [ ] Apply `[EnableRateLimiting]` to `DiagnosticsController`
- [ ] Apply `[EnableRateLimiting]` to remaining controllers
- [ ] Add configuration to `appsettings.json`
- [ ] Add logging for rate limit events
- [ ] Write integration tests
- [ ] Document API rate limits for frontend team

---

## Estimated Implementation Time

| Task | Time |
|------|------|
| Program.cs configuration | 30 min |
| Controller attributes | 45 min |
| appsettings.json | 15 min |
| Testing | 1 hour |
| Documentation | 30 min |
| **Total** | **~3 hours** |

---

*Document created: Rate Limiting Plan v1.0*
*Target: AdvisorySystem.Api (.NET 8)*
