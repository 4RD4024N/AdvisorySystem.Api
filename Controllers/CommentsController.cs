using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("standard")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notificationService;

    public CommentsController(
      AppDbContext db, 
        UserManager<AppUser> userManager,
        INotificationService notificationService)
    {
 _db = db;
 _userManager = userManager;
        _notificationService = notificationService;
    }

  private string GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(sub)) return sub;
     var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
     if (!string.IsNullOrEmpty(nameId)) return nameId;
        return User.Identity?.Name ?? throw new UnauthorizedAccessException("User ID not found");
    }

    // Belirli bir versiyon için yorumlarý listele
    [HttpGet("version/{versionId:int}")]
    public async Task<IActionResult> GetByVersion(int versionId)
    {
        var comments = await _db.Comments
            .Where(c => c.DocumentVersionId == versionId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
   return Ok(comments);
    }

    // Yorum ekle
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
    {
        var uid = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var isAdvisor = User.IsInRole("Advisor");
        
     var version = await _db.DocumentVersions
          .Include(v => v.Document)
      .FirstOrDefaultAsync(v => v.Id == dto.DocumentVersionId);
  
        if (version == null) return NotFound("Version not found");

 var documentOwner = await _userManager.FindByIdAsync(version.Document.OwnerUserId);
        if (documentOwner == null) return NotFound("Document owner not found");

        bool canComment = false;

  if (isAdmin)
        {
            canComment = true;
    }
        else if (version.Document.OwnerUserId == uid)
        {
       canComment = true;
   }
        else if (isAdvisor)
        {
    if (documentOwner.AdvisorId == uid)
            {
      canComment = true;
            }
  }

        if (!canComment)
            return Forbid();

        var comment = new Comment
    {
            DocumentVersionId = dto.DocumentVersionId,
     AuthorUserId = uid,
            Content = dto.Content
    };

     _db.Comments.Add(comment);
     await _db.SaveChangesAsync();

        if (version.Document.OwnerUserId != uid)
        {
    await _notificationService.CreateNotificationAsync(
         version.Document.OwnerUserId,
        "New Comment",
                $"A new comment was added to your document: {version.Document.Title}",
      NotificationType.NewComment,
  version.Document.Id.ToString(),
       "Document"
       );
   }

        if (!string.IsNullOrEmpty(documentOwner.AdvisorId) && documentOwner.AdvisorId != uid)
      {
            await _notificationService.CreateNotificationAsync(
     documentOwner.AdvisorId,
           "New Comment",
       $"A new comment was added to document: {version.Document.Title}",
           NotificationType.NewComment,
       version.Document.Id.ToString(),
  "Document"
     );
      }

        return Ok(new { comment.Id, comment.CreatedAt });
    }

    // Yorumu sil
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = GetUserId();
        var comment = await _db.Comments.FindAsync(id);
        
        if (comment == null) return NotFound();
        
     if (comment.AuthorUserId != uid && !User.IsInRole("Admin"))
    return Forbid();

        _db.Comments.Remove(comment);
await _db.SaveChangesAsync();

        return Ok(new { message = "Comment deleted" });
    }

    public record CreateCommentDto(int DocumentVersionId, string Content);
}
