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
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CommentsController(AppDbContext db)
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
        
        // Versiyonun varlýðýný kontrol et
        var version = await _db.DocumentVersions
            .Include(v => v.Document)
 .FirstOrDefaultAsync(v => v.Id == dto.DocumentVersionId);
  
     if (version == null) return NotFound("Version not found");

        // Yetki kontrolü: sadece sahibi, danýþmaný veya admin yorum yapabilir
        if (version.Document.OwnerUserId != uid && 
          version.Document.AdvisorUserId != uid && 
          !User.IsInRole("Admin"))
            return Forbid();

 var comment = new Comment
    {
     DocumentVersionId = dto.DocumentVersionId,
            AuthorUserId = uid,
            Content = dto.Content
        };

    _db.Comments.Add(comment);
  await _db.SaveChangesAsync();

   return Ok(new { comment.Id, comment.CreatedAt });
    }

    // Yorumu sil
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
 {
        var uid = GetUserId();
 var comment = await _db.Comments.FindAsync(id);
        
        if (comment == null) return NotFound();
        
        // Sadece yorum sahibi veya admin silebilir
        if (comment.AuthorUserId != uid && !User.IsInRole("Admin"))
   return Forbid();

     _db.Comments.Remove(comment);
    await _db.SaveChangesAsync();

 return Ok(new { message = "Comment deleted" });
    }

    public record CreateCommentDto(int DocumentVersionId, string Content);
}
