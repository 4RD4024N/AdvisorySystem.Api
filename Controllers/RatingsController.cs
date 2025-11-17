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
public class RatingsController : ControllerBase
{
    private readonly AppDbContext _db;
private readonly ILogger<RatingsController> _logger;

    public RatingsController(AppDbContext db, ILogger<RatingsController> logger)
    {
 _db = db;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? User.FindFirstValue("sub")
     ?? throw new UnauthorizedAccessException("User ID not found");
    }

    // Create or update rating (Advisor/Admin only)
    [HttpPost]
[Authorize(Roles = "Advisor,Admin")]
    public async Task<IActionResult> CreateOrUpdateRating([FromBody] CreateRatingDto dto)
{
        try
        {
 var userId = GetUserId();

            // Validate document version exists
         var version = await _db.DocumentVersions
     .Include(v => v.Document)
    .FirstOrDefaultAsync(v => v.Id == dto.DocumentVersionId);

  if (version == null)
            {
    return NotFound(new { error = "Document version not found" });
       }

  // Validate score range
  if (dto.Score < 1 || dto.Score > 100)
  {
    return BadRequest(new { error = "Score must be between 1 and 100" });
 }

   // Check if advisor is assigned to this document or is admin
     var isAdmin = User.IsInRole("Admin");
    var isAssignedAdvisor = version.Document.AdvisorUserId == userId;

        if (!isAdmin && !isAssignedAdvisor)
       {
  return Forbid();
 }

  // Check if rating already exists
  var existingRating = await _db.DocumentRatings
 .FirstOrDefaultAsync(r => r.DocumentVersionId == dto.DocumentVersionId && r.AdvisorUserId == userId);

 if (existingRating != null)
     {
     // Update existing rating
   existingRating.Score = dto.Score;
 existingRating.Comments = dto.Comments;
         existingRating.CreatedAt = DateTime.UtcNow; // Update timestamp
        await _db.SaveChangesAsync();

          return Ok(new
    {
             message = "Rating updated successfully",
           ratingId = existingRating.Id,
       existingRating.Score
     });
   }

 // Create new rating
   var rating = new DocumentRating
   {
    DocumentVersionId = dto.DocumentVersionId,
     AdvisorUserId = userId,
          Score = dto.Score,
  Comments = dto.Comments
     };

            _db.DocumentRatings.Add(rating);
   await _db.SaveChangesAsync();

 return Ok(new
            {
    message = "Rating created successfully",
       ratingId = rating.Id,
     rating.Score
    });
        }
      catch (Exception ex)
 {
     _logger.LogError(ex, "Failed to create/update rating");
     return StatusCode(500, new { error = "Failed to save rating", details = ex.Message });
 }
    }

    // Get rating for a document version
    [HttpGet("version/{versionId}")]
    public async Task<IActionResult> GetRatingForVersion(int versionId)
    {
   try
   {
   // Get all ratings for this version
         var ratings = await _db.DocumentRatings
       .Where(r => r.DocumentVersionId == versionId)
       .Select(r => new
  {
      r.Id,
       r.DocumentVersionId,
        r.AdvisorUserId,
  r.Score,
            r.Comments,
       r.CreatedAt
          })
    .ToListAsync();

   if (!ratings.Any())
   {
      return Ok(new
      {
     hasRating = false,
           averageScore = (double?)null,
       ratings = new List<object>()
  });
}

            var averageScore = ratings.Average(r => r.Score);

         return Ok(new
    {
    hasRating = true,
     averageScore,
   ratingCount = ratings.Count,
   ratings
    });
        }
  catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to get ratings");
     return StatusCode(500, new { error = "Failed to retrieve ratings", details = ex.Message });
        }
    }

    // Get all ratings by an advisor
    [HttpGet("by-advisor/{advisorId}")]
 [Authorize(Roles = "Admin,Advisor")]
    public async Task<IActionResult> GetRatingsByAdvisor(string advisorId)
    {
      try
  {
var userId = GetUserId();

    // Only admin or the advisor themselves can view
 if (!User.IsInRole("Admin") && userId != advisorId)
         {
             return Forbid();
 }

            var ratings = await _db.DocumentRatings
   .Where(r => r.AdvisorUserId == advisorId)
                .Include(r => r.DocumentVersion)
      .ThenInclude(v => v.Document)
   .OrderByDescending(r => r.CreatedAt)
       .Select(r => new
    {
     r.Id,
       r.DocumentVersionId,
   documentTitle = r.DocumentVersion.Document.Title,
  versionNo = r.DocumentVersion.VersionNo,
     r.Score,
            r.Comments,
    r.CreatedAt
      })
    .ToListAsync();

return Ok(new
     {
     totalRatings = ratings.Count,
      averageScore = ratings.Any() ? ratings.Average(r => r.Score) : 0,
     ratings
     });
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to get advisor ratings");
     return StatusCode(500, new { error = "Failed to retrieve ratings", details = ex.Message });
        }
    }

    // Get ratings for my documents (Student)
    [HttpGet("my-documents")]
    public async Task<IActionResult> GetMyDocumentRatings()
    {
  try
  {
       var userId = GetUserId();

   // Get all document versions owned by the user with their ratings
       var documentVersions = await _db.DocumentVersions
          .Where(v => v.Document.OwnerUserId == userId)
      .Include(v => v.Document)
      .Select(v => new
     {
  v.Id,
           documentId = v.Document.Id,
      documentTitle = v.Document.Title,
    v.VersionNo,
     ratings = _db.DocumentRatings
   .Where(r => r.DocumentVersionId == v.Id)
       .Select(r => new
               {
         r.Id,
     r.AdvisorUserId,
          r.Score,
    r.Comments,
   r.CreatedAt
      })
  .ToList()
      })
     .Where(v => v.ratings.Any()) // Only versions with ratings
       .ToListAsync();

   return Ok(documentVersions);
        }
        catch (Exception ex)
 {
     _logger.LogError(ex, "Failed to get document ratings");
  return StatusCode(500, new { error = "Failed to retrieve ratings", details = ex.Message });
  }
 }

    // Delete rating (Admin or rating author)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRating(int id)
    {
   try
        {
 var userId = GetUserId();
       var rating = await _db.DocumentRatings.FindAsync(id);

    if (rating == null)
   {
   return NotFound(new { error = "Rating not found" });
   }

   // Only admin or rating author can delete
     if (!User.IsInRole("Admin") && rating.AdvisorUserId != userId)
  {
     return Forbid();
    }

   _db.DocumentRatings.Remove(rating);
 await _db.SaveChangesAsync();

    return Ok(new { message = "Rating deleted successfully" });
        }
catch (Exception ex)
        {
         _logger.LogError(ex, "Failed to delete rating");
 return StatusCode(500, new { error = "Failed to delete rating", details = ex.Message });
        }
    }

    // DTOs
    public record CreateRatingDto(
  int DocumentVersionId,
      int Score,
    string? Comments
    );
}
