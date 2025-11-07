using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatisticsController(AppDbContext db)
    {
        _db = db;
    }

    private string GetUserId()
    {
   var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
  if (!string.IsNullOrEmpty(sub)) return sub;
      var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (!string.IsNullOrEmpty(nameId)) return nameId;
 return User.Identity?.Name ?? throw new UnauthorizedAccessException("User ID not found");
    }

  // Öðrenci için özet istatistikler
    [HttpGet("student/summary")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentSummary()
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
      TotalDocuments = totalDocuments,
       TotalVersions = totalVersions,
 PendingSubmissions = pendingSubmissions,
CompletedSubmissions = completedSubmissions
  });
    }

  // Danýþman için özet istatistikler
    [HttpGet("advisor/summary")]
    [Authorize(Roles = "Advisor")]
    public async Task<IActionResult> GetAdvisorSummary()
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
       AssignedDocuments = assignedDocuments,
   TotalComments = totalComments,
          StudentsCount = studentsCount
        });
    }

    // Admin için genel istatistikler
    [HttpGet("admin/overview")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminOverview()
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
        d.Id,
   d.Title,
          d.CreatedAt,
    OwnerUserId = d.OwnerUserId
            })
        .ToListAsync();

        return Ok(new
    {
TotalDocuments = totalDocuments,
   TotalVersions = totalVersions,
TotalSubmissions = totalSubmissions,
   TotalComments = totalComments,
RecentActivity = recentActivity
     });
    }
}
