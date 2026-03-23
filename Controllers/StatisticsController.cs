using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("admin")]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(AppDbContext db, ILogger<StatisticsController> logger)
    {
   _db = db;
        _logger = logger;
    }

    private string GetUserId()
    {
        try
        {
 var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
     if (!string.IsNullOrEmpty(sub)) return sub;
         
            var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(nameId)) return nameId;
            
 var name = User.Identity?.Name;
        if (!string.IsNullOrEmpty(name)) return name;

            _logger.LogError("User ID not found in claims");
            throw new UnauthorizedAccessException("User ID not found");
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error getting user ID");
 throw;
        }
    }

    // Öðrenci için özet istatistikler - Herkes kendi istatistiklerini görebilir
  [HttpGet("student/summary")]
    public async Task<IActionResult> GetStudentSummary()
  {
        try
    {
          var uid = GetUserId();
            
        var totalDocuments = await _db.Documents.CountAsync(d => d.OwnerUserId == uid);
         var totalVersions = await _db.DocumentVersions
            .Where(v => v.Document.OwnerUserId == uid)
.CountAsync();
        
  var pendingSubmissions = await _db.Submissions
 .CountAsync(s => s.StudentId == uid && s.Status == "Pending");
 
 var completedSubmissions = await _db.Submissions
            .CountAsync(s => s.StudentId == uid && s.Status == "Completed");

         return Ok(new
            {
                totalDocuments = totalDocuments,
 totalVersions = totalVersions,
       pendingSubmissions = pendingSubmissions,
       completedSubmissions = completedSubmissions
        });
        }
      catch (Exception ex)
   {
            _logger.LogError(ex, "Failed to get student summary");
      return StatusCode(500, new { error = "Failed to retrieve statistics", details = ex.Message });
        }
    }

    // Danýþman için özet istatistikler
    [HttpGet("advisor/summary")]
    [Authorize(Roles = "Advisor,Admin")]
    public async Task<IActionResult> GetAdvisorSummary()
    {
   try
      {
 var uid = GetUserId();
            
            var assignedDocuments = await _db.Documents
  .CountAsync(d => d.AdvisorUserId == uid);
            
            var totalComments = await _db.Comments
          .CountAsync(c => c.AuthorUserId == uid);
         
            var studentsCount = await _db.Documents
       .Where(d => d.AdvisorUserId == uid)
 .Select(d => d.OwnerUserId)
       .Distinct()
            .CountAsync();

            return Ok(new
  {
                assignedDocuments = assignedDocuments,
      totalComments = totalComments,
         studentsCount = studentsCount
            });
    }
     catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to get advisor summary");
            return StatusCode(500, new { error = "Failed to retrieve statistics" });
        }
    }

    // Admin için genel istatistikler
    [HttpGet("admin/overview")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminOverview()
    {
        try
   {
            var totalDocuments = await _db.Documents.CountAsync();
     var totalVersions = await _db.DocumentVersions.CountAsync();
  var totalSubmissions = await _db.Submissions.CountAsync();
      var totalComments = await _db.Comments.CountAsync();
  
        var recentActivity = await _db.Documents
     .OrderByDescending(d => d.CreatedAt)
                .Take(10)
            .Select(d => new
          {
      id = d.Id,
     title = d.Title,
          createdAt = d.CreatedAt,
     ownerUserId = d.OwnerUserId
       })
         .ToListAsync();

         return Ok(new
    {
     totalDocuments = totalDocuments,
       totalVersions = totalVersions,
       totalSubmissions = totalSubmissions,
       totalComments = totalComments,
    recentActivity = recentActivity
          });
    }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to get admin overview");
            return StatusCode(500, new { error = "Failed to retrieve statistics" });
        }
    }
}
