# ? Performance Optimization Plan - AdvisorySystem.Api

## Current Performance Bottlenecks (Identified)

### 1. Database Query Optimization

#### Problem: N+1 Query Issues

```csharp
// ? BAD: N+1 Query Problem
var courses = await _db.Courses.ToListAsync();
foreach (var course in courses)
{
    // Each iteration causes a separate query!
    var category = await _db.CourseCategories.FindAsync(course.CategoryId);
}

// ? GOOD: Eager Loading
var courses = await _db.Courses
 .Include(c => c.Category)
    .ToListAsync();
```

#### Fix: Add Explicit Includes

Update `CoursesController.cs`:
```csharp
[HttpGet]
public async Task<IActionResult> GetAllCourses()
{
    var courses = await _db.Courses
        .Include(c => c.Category)  // ? Add this
  .AsSplitQuery()    // ? For multiple includes
        .ToListAsync();
}
```

### 2. Implement Caching

#### 2.1 Response Caching

```bash
dotnet add package Microsoft.AspNetCore.ResponseCaching
```

```csharp
// Program.cs
builder.Services.AddResponseCaching();

// Add before app.UseRateLimiter()
app.UseResponseCaching();

// Controllers - Add caching attributes
[HttpGet]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "categoryId", "semester" })]
public async Task<IActionResult> GetAllCourses()
{
    // Will cache for 5 minutes
}
```

#### 2.2 Distributed Caching with Redis

```bash
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
     options.Configuration = builder.Configuration["Redis:ConnectionString"];
        options.InstanceName = "AdvisorySystem:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Services/CachedCourseService.cs
public class CachedCourseService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<List<Course>> GetCoursesAsync()
    {
        const string cacheKey = "courses:all";
     
   var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
          return JsonSerializer.Deserialize<List<Course>>(cachedData)!;
  }

        var courses = await _db.Courses
     .Include(c => c.Category)
    .ToListAsync();

        await _cache.SetStringAsync(
            cacheKey,
JsonSerializer.Serialize(courses),
      new DistributedCacheEntryOptions
       {
        AbsoluteExpirationRelativeToNow = CacheDuration
    });

return courses;
    }
}
```

### 3. Database Indexing

```csharp
// Add to AppDbContext.OnModelCreating

protected override void OnModelCreating(ModelBuilder b)
{
    base.OnModelCreating(b);

  // ? Add indexes for frequently queried columns
    
    // Courses
    b.Entity<Course>()
        .HasIndex(c => c.Semester);
    
    b.Entity<Course>()
        .HasIndex(c => c.IsElective);
    
    b.Entity<Course>()
    .HasIndex(c => new { c.CategoryId, c.Semester }); // Composite index

    // CourseSchedules
    b.Entity<CourseSchedule>()
  .HasIndex(cs => new { cs.CourseId, cs.SectionCode });
    
    b.Entity<CourseSchedule>()
        .HasIndex(cs => new { cs.Semester, cs.DayOfWeek, cs.StartTime });

    // Documents
    b.Entity<Document>()
     .HasIndex(d => d.OwnerUserId);
    
    b.Entity<Document>()
 .HasIndex(d => d.AdvisorUserId);
    
    b.Entity<Document>()
        .HasIndex(d => d.CreatedAt);

    // StudentCourses
    b.Entity<StudentCourse>()
        .HasIndex(sc => new { sc.StudentId, sc.CourseId });
    
    b.Entity<StudentCourse>()
        .HasIndex(sc => sc.IsCompleted);

    // Notifications
    b.Entity<Notification>()
   .HasIndex(n => new { n.UserId, n.IsRead });

    // Full-text search index for course descriptions (SQL Server)
    // Execute this SQL manually:
    // CREATE FULLTEXT INDEX ON Courses(CourseName, Description)
    // KEY INDEX PK_Courses
}
```

Create migration:
```bash
dotnet ef migrations add AddPerformanceIndexes
dotnet ef database update
```

### 4. Async/Await Optimization

```csharp
// ? BAD: Blocking calls
public IActionResult GetCourses()
{
    var courses = _db.Courses.ToList(); // Blocks thread
    return Ok(courses);
}

// ? GOOD: Async all the way
public async Task<IActionResult> GetCourses()
{
    var courses = await _db.Courses.ToListAsync(); // Non-blocking
    return Ok(courses);
}

// ? BAD: Sync over async
public async Task Process()
{
    var result = DoSomethingAsync().Result; // DEADLOCK RISK!
}

// ? GOOD: Await properly
public async Task Process()
{
    var result = await DoSomethingAsync();
}
```

### 5. Pagination Implementation

```csharp
// Models/PagedResult.cs
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

// Extensions/QueryableExtensions.cs
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
   this IQueryable<T> query,
        int pageNumber,
     int pageSize)
    {
    var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
       .ToListAsync();

        return new PagedResult<T>
        {
       Items = items,
        TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

// Update Controllers
[HttpGet]
public async Task<IActionResult> GetCourses(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20)
{
    if (pageSize > 100) pageSize = 100; // Limit max page size

    var result = await _db.Courses
   .Include(c => c.Category)
        .OrderBy(c => c.CourseCode)
        .ToPagedResultAsync(pageNumber, pageSize);

    return Ok(result);
}
```

### 6. Optimize File Operations

```csharp
// Services/OptimizedFileStorage.cs
public class OptimizedAzureBlobStorage : IFileStorage
{
  private readonly BlobServiceClient _client;
    private readonly IMemoryCache _cache;

    public async Task<Stream> OpenAsync(string path)
    {
        // Cache small files in memory
        var cacheKey = $"file:{path}";
        if (_cache.TryGetValue<byte[]>(cacheKey, out var cachedData))
        {
            return new MemoryStream(cachedData);
        }

     var container = _client.GetBlobContainerClient("documents");
        var blob = container.GetBlobClient(path);
        
        var download = await blob.DownloadAsync();
        
        // Cache files smaller than 1MB
        if (download.Value.ContentLength < 1024 * 1024)
  {
            using var ms = new MemoryStream();
          await download.Value.Content.CopyToAsync(ms);
       var bytes = ms.ToArray();
       
   _cache.Set(cacheKey, bytes, TimeSpan.FromMinutes(10));
 return new MemoryStream(bytes);
        }

        return download.Value.Content;
    }

    // Use streaming for uploads
    public async Task<(string path, long size)> SaveAsync(IFormFile file, string prefix)
    {
        var container = _client.GetBlobContainerClient("documents");
        var blobName = $"{prefix}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blob = container.GetBlobClient(blobName);

        // Stream directly, don't load into memory
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = file.ContentType
  });

        return (blobName, file.Length);
    }
}
```

### 7. Optimize JWT Generation

```csharp
// Cache SecurityKey to avoid recreating
public class OptimizedAuthController : AuthController
{
    private static readonly Lazy<SymmetricSecurityKey> _signingKey = new(() =>
    {
        var configuration = /* get from DI */;
        var keyBytes = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
        return new SymmetricSecurityKey(keyBytes);
    });

    private async Task<string> GenerateTokenAsync(AppUser user)
    {
        // Reuse key instead of creating new one each time
  var key = _signingKey.Value;
        
        // Batch role loading
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>(roles.Count + 5) // Pre-size list
      {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
         new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
   };
        
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
 var expires = DateTime.UtcNow.AddMinutes(1440);

        var token = new JwtSecurityToken(
issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
   claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 8. Connection String Optimization

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Default": "Server=...;Database=...;User Id=...;Password=...;MultipleActiveResultSets=true;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30;TrustServerCertificate=True;"
  }
}
```

### 9. Bulk Operations

```csharp
// For large data operations, use bulk inserts
public async Task BulkInsertCoursesAsync(List<Course> courses)
{
    // Instead of:
    // foreach (var course in courses)
    // {
    //     _db.Courses.Add(course);
    // await _db.SaveChangesAsync(); // ? N queries
  // }

    // Do this:
    _db.Courses.AddRange(courses);
    await _db.SaveChangesAsync(); // ? Single batch

    // Or use EFCore.BulkExtensions for very large datasets
    // await _db.BulkInsertAsync(courses);
}
```

### 10. Query Projection

```csharp
// ? BAD: Loading entire entities when you only need some fields
public async Task<IActionResult> GetCourseNames()
{
    var courses = await _db.Courses
        .Include(c => c.Category)
        .ToListAsync(); // Loads ALL columns
    
    return Ok(courses.Select(c => new { c.Id, c.CourseName }));
}

// ? GOOD: Project only what you need
public async Task<IActionResult> GetCourseNames()
{
    var courses = await _db.Courses
  .Select(c => new { c.Id, c.CourseName })
      .ToListAsync(); // Only loads Id and CourseName

    return Ok(courses);
}
```

### 11. AsNoTracking for Read-Only Queries

```csharp
// For queries where you don't need change tracking
[HttpGet]
public async Task<IActionResult> GetCourses()
{
    var courses = await _db.Courses
        .AsNoTracking() // ? 30% faster for read-only
        .Include(c => c.Category)
        .ToListAsync();

    return Ok(courses);
}
```

### 12. Database Connection Resilience

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions =>
{
            // Retry on transient failures
     sqlOptions.EnableRetryOnFailure(
      maxRetryCount: 5,
    maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);

    // Command timeout
    sqlOptions.CommandTimeout(60);

   // Query splitting for better performance
  sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
  });
});
```

### 13. Compression

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// Add before UseStaticFiles
app.UseResponseCompression();
```

### 14. Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddAzureBlobStorage(
        builder.Configuration["Azure:StorageConnectionString"] ?? "",
        name: "azure-blob-storage")
    .AddCheck("rate-limiter", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
 Predicate = check => check.Tags.Contains("ready")
});
```

## Performance Monitoring

### Application Insights Query Examples

```kusto
// Average response time by endpoint
requests
| where timestamp > ago(24h)
| summarize avg(duration), count() by name
| order by avg_duration desc

// Slowest queries
dependencies
| where type == "SQL"
| where timestamp > ago(24h)
| summarize avg(duration), max(duration) by name
| order by avg_duration desc

// Rate limiting violations
traces
| where message contains "Rate limit exceeded"
| where timestamp > ago(24h)
| summarize count() by tostring(customDimensions.Endpoint)

// Failed requests
requests
| where success == false
| where timestamp > ago(24h)
| summarize count() by resultCode, name
```

## Performance Testing Results

### Baseline (Before Optimization)
```
Endpoint: GET /api/courses
- P50: 250ms
- P95: 800ms
- P99: 1500ms
- Throughput: 50 req/s
```

### Target (After Optimization)
```
Endpoint: GET /api/courses
- P50: < 100ms
- P95: < 300ms
- P99: < 500ms
- Throughput: > 200 req/s
```

## Database Performance Tuning

```sql
-- Check missing indexes
SELECT 
    OBJECT_NAME(d.object_id) AS TableName,
    d.equality_columns,
    d.inequality_columns,
    d.included_columns,
    d.avg_user_impact,
    d.user_seeks
FROM sys.dm_db_missing_index_details d
INNER JOIN sys.dm_db_missing_index_groups g ON d.index_handle = g.index_handle
INNER JOIN sys.dm_db_missing_index_group_stats s ON g.index_group_handle = s.group_handle
ORDER BY d.avg_user_impact DESC;

-- Check slow queries
SELECT TOP 20
qs.execution_count,
    qs.total_elapsed_time / 1000000 AS total_elapsed_time_seconds,
    qs.total_elapsed_time / qs.execution_count / 1000000 AS avg_elapsed_time_seconds,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
      ((CASE qs.statement_end_offset
       WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY qs.total_elapsed_time DESC;

-- Update statistics
EXEC sp_updatestats;

-- Rebuild fragmented indexes
ALTER INDEX ALL ON Courses REBUILD;
ALTER INDEX ALL ON Documents REBUILD;
```

## CDN Integration (Future)

```csharp
// For serving static files and uploaded documents
public class CdnUrlService
{
  private readonly IConfiguration _configuration;
    private readonly string _cdnBaseUrl;

    public CdnUrlService(IConfiguration configuration)
    {
        _configuration = configuration;
        _cdnBaseUrl = configuration["Cdn:BaseUrl"] ?? "";
    }

    public string GetFileUrl(string path)
 {
        if (string.IsNullOrEmpty(_cdnBaseUrl))
   return path;

        return $"{_cdnBaseUrl}/{path}";
    }
}
```

## Performance Checklist

- [ ] Add database indexes
- [ ] Implement response caching
- [ ] Add Redis distributed cache
- [ ] Enable response compression
- [ ] Use AsNoTracking for read queries
- [ ] Implement pagination
- [ ] Optimize JWT generation
- [ ] Add health checks
- [ ] Configure connection pooling
- [ ] Enable query splitting
- [ ] Add Application Insights monitoring
- [ ] Run load tests
- [ ] Optimize N+1 queries
- [ ] Profile slow endpoints
- [ ] Set up CDN (if needed)
