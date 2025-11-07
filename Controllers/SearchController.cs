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
public class SearchController : ControllerBase
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db)
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

    // Doküman arama
    [HttpGet("documents")]
    public async Task<IActionResult> SearchDocuments(
        [FromQuery] string? query,
        [FromQuery] string? tags,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
[FromQuery] int pageSize = 20)
    {
        var uid = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var isAdvisor = User.IsInRole("Advisor");

        var documentsQuery = _db.Documents.AsQueryable();

    // Yetki filtresi
   if (!isAdmin)
  {
          if (isAdvisor)
            {
 // Danýþman hem kendisine atananlarý hem kendi dokümanlarýný görsün
     documentsQuery = documentsQuery.Where(d => 
   d.AdvisorUserId == uid || d.OwnerUserId == uid);
            }
    else
      {
         // Öðrenci sadece kendi dokümanlarýný görsün
           documentsQuery = documentsQuery.Where(d => d.OwnerUserId == uid);
   }
        }

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(query))
        {
            documentsQuery = documentsQuery.Where(d => 
     d.Title.Contains(query) || 
         (d.Tags != null && d.Tags.Contains(query)));
   }

        // Tag filtresi
   if (!string.IsNullOrWhiteSpace(tags))
     {
            documentsQuery = documentsQuery.Where(d => 
       d.Tags != null && d.Tags.Contains(tags));
        }

     // Tarih filtresi
   if (startDate.HasValue)
        {
            documentsQuery = documentsQuery.Where(d => d.CreatedAt >= startDate.Value);
        }
      
        if (endDate.HasValue)
 {
   documentsQuery = documentsQuery.Where(d => d.CreatedAt <= endDate.Value);
      }

        var totalCount = await documentsQuery.CountAsync();
        
  var documents = await documentsQuery
    .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
      .Take(pageSize)
    .Select(d => new
      {
    d.Id,
  d.Title,
     d.Tags,
   d.CreatedAt,
      OwnerUserId = d.OwnerUserId,
   AdvisorUserId = d.AdvisorUserId,
  VersionCount = d.Versions.Count
     })
      .ToListAsync();

      return Ok(new
   {
   TotalCount = totalCount,
       Page = page,
      PageSize = pageSize,
TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
   Documents = documents
        });
    }

    // Tag'lere göre doküman sayýsý
    [HttpGet("tags/popular")]
    public async Task<IActionResult> GetPopularTags([FromQuery] int top = 10)
 {
   var uid = GetUserId();
        var isAdmin = User.IsInRole("Admin");
   var isAdvisor = User.IsInRole("Advisor");

        var documentsQuery = _db.Documents.AsQueryable();

   // Yetki filtresi
        if (!isAdmin)
   {
            if (isAdvisor)
            {
           documentsQuery = documentsQuery.Where(d => 
   d.AdvisorUserId == uid || d.OwnerUserId == uid);
            }
     else
          {
           documentsQuery = documentsQuery.Where(d => d.OwnerUserId == uid);
        }
        }

        var tagsWithCounts = await documentsQuery
   .Where(d => d.Tags != null && d.Tags != "")
    .Select(d => d.Tags!)
    .ToListAsync();

        // Tag'leri ayýr ve say
    var tagDictionary = new Dictionary<string, int>();
 foreach (var tagString in tagsWithCounts)
        {
       var tags = tagString.Split(',', StringSplitOptions.RemoveEmptyEntries);
  foreach (var tag in tags)
   {
   var trimmedTag = tag.Trim();
     if (!string.IsNullOrEmpty(trimmedTag))
  {
   if (tagDictionary.ContainsKey(trimmedTag))
  tagDictionary[trimmedTag]++;
 else
      tagDictionary[trimmedTag] = 1;
   }
            }
        }

        var popularTags = tagDictionary
        .OrderByDescending(kvp => kvp.Value)
            .Take(top)
            .Select(kvp => new { Tag = kvp.Key, Count = kvp.Value })
    .ToList();

 return Ok(popularTags);
    }
}
