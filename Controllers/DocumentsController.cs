using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _users;
        private readonly IFileStorage _storage;

        public DocumentsController(AppDbContext db, UserManager<AppUser> users, IFileStorage storage)
        { _db = db; _users = users; _storage = storage; }

        // Helper method to get user ID from claims
        private string GetUserId()
        {
            // Try standard "sub" claim first (JWT standard)
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!string.IsNullOrEmpty(sub)) return sub;
       
            // Fallback to ClaimTypes.NameIdentifier (ASP.NET Identity default)
            var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(nameId)) return nameId;
   
            // Last resort: User.Identity.Name
            return User.Identity?.Name ?? throw new UnauthorizedAccessException("User ID not found in token");
        }

        // Öğrenci kendi dokümanlarını görsün
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var uid = GetUserId();
            var list = await _db.Documents
                .Where(d => d.OwnerUserId == uid)
                .Select(d => new {
                    d.Id,
                    d.Title,
                    d.Tags,
                    d.CreatedAt,
                    VersionCount = d.Versions.Count
                })
                .ToListAsync();
            return Ok(list);
        }

        // Öğrenci yeni Document açar
        public record CreateDocDto(string Title, string? Tags);
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(CreateDocDto dto)
        {
            var uid = GetUserId();
            var doc = new Document { Title = dto.Title, Tags = dto.Tags, OwnerUserId = uid };
            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();
            return Ok(new { doc.Id });
        }

        // Versiyon yükleme
        [HttpPost("{id:int}/versions")]
        [Authorize] // role kontrolü: owner veya advisor olabilir
        public async Task<IActionResult> Upload(int id, IFormFile file, string? notes)
        {
            if (file is null || file.Length == 0) return BadRequest("Dosya yok.");
            var doc = await _db.Documents.Include(x => x.Versions).FirstOrDefaultAsync(x => x.Id == id);
            if (doc is null) return NotFound();

            var uid = GetUserId();
            // Yetki: sahibi veya danışmanı
            if (uid != doc.OwnerUserId && uid != doc.AdvisorUserId && !User.IsInRole("Admin"))
                return Forbid();

            var versionNo = (doc.Versions.Max(v => (int?)v.VersionNo) ?? 0) + 1;
            var result = await _storage.SaveAsync(file, $"doc-{doc.Id}");
            var path = result.path;
            var size = result.size;

            var ver = new DocumentVersion
            {
                DocumentId = doc.Id,
                VersionNo = versionNo,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = size,
                StoragePath = path,
                UploadedByUserId = uid,
                Notes = notes
            };
            _db.DocumentVersions.Add(ver);
            await _db.SaveChangesAsync();

            return Ok(new { ver.Id, ver.VersionNo });
        }

        // Versiyonları listele
        [HttpGet("{id:int}/versions")]
        [Authorize]
        public async Task<IActionResult> Versions(int id)
        {
            var list = await _db.DocumentVersions
                .Where(v => v.DocumentId == id)
                .OrderByDescending(v => v.VersionNo)
                .Select(v => new { v.Id, v.VersionNo, v.FileName, v.Size, v.CreatedAt, v.Notes })
                .ToListAsync();
            return Ok(list);
        }

        // Dosyayı indir
        [HttpGet("download/{versionId:int}")]
        [Authorize]
        public async Task<IActionResult> Download(int versionId)
        {
            var v = await _db.DocumentVersions.FindAsync(versionId);
            if (v is null) return NotFound();
            var stream = _storage.Open(v.StoragePath);
            return File(stream, v.ContentType, fileDownloadName: v.FileName);
        }
    }
}
