using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdvisorySystem.Api.Data;
using System.Diagnostics;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<HealthController> _logger;
    private readonly IConfiguration _configuration;

    public HealthController(
        AppDbContext db,
    ILogger<HealthController> logger,
        IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
    }

    // Basic health check (public)
    [HttpGet]
    public IActionResult HealthCheck()
    {
    return Ok(new
        {
          status = "healthy",
 timestamp = DateTime.UtcNow,
            version = "1.0.0",
 environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
        });
    }

    // Detailed health check (admin only)
    [HttpGet("detailed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DetailedHealthCheck()
    {
        try
        {
       var checks = new Dictionary<string, object>();

            // Database check
  try
          {
       var dbCheck = await _db.Database.CanConnectAsync();
 var userCount = await _db.Users.CountAsync();
      checks["database"] = new
 {
        status = dbCheck ? "healthy" : "unhealthy",
          canConnect = dbCheck,
   userCount = userCount
     };
  }
      catch (Exception ex)
        {
           checks["database"] = new
  {
         status = "unhealthy",
    error = ex.Message
   };
          }


// Memory check
       try
   {
     var process = Process.GetCurrentProcess();
      checks["memory"] = new
     {
   workingSetMB = process.WorkingSet64 / 1024.0 / 1024.0,
     privateMemoryMB = process.PrivateMemorySize64 / 1024.0 / 1024.0
     };
   }
  catch (Exception ex)
    {
     checks["memory"] = new
  {
    status = "error",
  error = ex.Message
    };
 }

       // Configuration check
            checks["configuration"] = new
 {
         jwtConfigured = !string.IsNullOrEmpty(_configuration["Jwt:Key"]),
        storageConfigured = !string.IsNullOrEmpty(_configuration["Storage:Root"]),
      corsConfigured = true
 };

   // Uptime
     try
 {
        var process = Process.GetCurrentProcess();
       checks["uptime"] = new
        {
    uptimeSeconds = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds,
startTime = process.StartTime.ToUniversalTime()
           };
  }
  catch (Exception ex)
  {
  checks["uptime"] = new
    {
    status = "error",
  error = ex.Message
                };
      }

  var overallHealthy = !checks.Values
   .OfType<IDictionary<string, object>>()
             .Any(c => c.ContainsKey("status") && c["status"]?.ToString() == "unhealthy");

return Ok(new
     {
     status = overallHealthy ? "healthy" : "unhealthy",
        timestamp = DateTime.UtcNow,
       checks = checks
 });
      }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Failed to perform detailed health check");
       return StatusCode(500, new { 
      error = "Health check failed", 
   details = ex.Message 
    });
        }
    }

    // Database connectivity check
    [HttpGet("database")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DatabaseCheck()
    {
        try
      {
     var canConnect = await _db.Database.CanConnectAsync();
    var pendingMigrations = await _db.Database.GetPendingMigrationsAsync();

  return Ok(new
{
        status = canConnect ? "healthy" : "unhealthy",
  canConnect = canConnect,
   hasPendingMigrations = pendingMigrations.Any(),
         pendingMigrations = pendingMigrations.ToList()
    });
  }
    catch (Exception ex)
    {
   _logger.LogError(ex, "Database health check failed");
       return StatusCode(500, new
       {
      status = "unhealthy",
   error = ex.Message,
   details = ex.InnerException?.Message
    });
        }
    }

    // Application metrics (admin only)
    [HttpGet("metrics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMetrics()
{
        try
     {
       var metrics = new Dictionary<string, object>();

      // Database metrics
            try
        {
   metrics["users"] = new
       {
        total = await _db.Users.CountAsync(),
      students = await _db.Users
         .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
         .Join(_db.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
               .Where(x => x.r.Name == "Student")
          .CountAsync()
   };
            }
         catch (Exception ex)
            {
         _logger.LogError(ex, "Failed to get user metrics");
    metrics["users"] = new { error = ex.Message };
            }

            try
  {
        metrics["documents"] = new
  {
total = await _db.Documents.CountAsync(),
     withAdvisor = await _db.Documents.CountAsync(d => d.AdvisorUserId != null)
};
     }
catch (Exception ex)
 {
  _logger.LogError(ex, "Failed to get document metrics");
     metrics["documents"] = new { error = ex.Message };
   }

          try
    {
   metrics["versions"] = new
        {
  total = await _db.DocumentVersions.CountAsync(),
   totalSizeMB = await _db.DocumentVersions.SumAsync(v => (double?)v.Size) / 1024.0 / 1024.0 ?? 0
          };
            }
            catch (Exception ex)
   {
        _logger.LogError(ex, "Failed to get version metrics");
         metrics["versions"] = new { error = ex.Message };
  }

     try
   {
         metrics["submissions"] = new
       {
      total = await _db.Submissions.CountAsync(),
       pending = await _db.Submissions.CountAsync(s => s.Status == "Pending"),
          completed = await _db.Submissions.CountAsync(s => s.Status == "Completed")
        };
}
            catch (Exception ex)
    {
    _logger.LogError(ex, "Failed to get submission metrics");
     metrics["submissions"] = new { error = ex.Message };
}

  try
     {
       metrics["comments"] = new
      {
   total = await _db.Comments.CountAsync()
      };
     }
            catch (Exception ex)
       {
       _logger.LogError(ex, "Failed to get comment metrics");
      metrics["comments"] = new { error = ex.Message };
            }

  try
{
       metrics["notifications"] = new
                {
  total = await _db.Notifications.CountAsync(),
         unread = await _db.Notifications.CountAsync(n => !n.IsRead)
     };
   }
  catch (Exception ex)
            {
   _logger.LogError(ex, "Failed to get notification metrics");
       metrics["notifications"] = new { error = ex.Message };
            }

            return Ok(new
      {
     timestamp = DateTime.UtcNow,
 metrics = metrics
       });
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Failed to get metrics");
 return StatusCode(500, new { 
    error = "Failed to retrieve metrics", 
      details = ex.Message 
       });
        }
    }

    // System information (admin only)
    [HttpGet("system")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetSystemInfo()
    {
        try
        {
   var process = Process.GetCurrentProcess();

       return Ok(new
        {
    dotnetVersion = Environment.Version.ToString(),
            osVersion = Environment.OSVersion.ToString(),
       machineName = Environment.MachineName,
            processorCount = Environment.ProcessorCount,
   workingSet = new
   {
       bytes = process.WorkingSet64,
  mb = process.WorkingSet64 / 1024.0 / 1024.0,
     gb = process.WorkingSet64 / 1024.0 / 1024.0 / 1024.0
     },
      uptime = new
    {
      seconds = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds,
   minutes = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMinutes,
          hours = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalHours,
        startTime = process.StartTime.ToUniversalTime()
   }
  });
      }
      catch (Exception ex)
        {
    _logger.LogError(ex, "Failed to get system information");
   return StatusCode(500, new { 
    error = "Failed to retrieve system information", 
   details = ex.Message 
  });
        }
    }
}
