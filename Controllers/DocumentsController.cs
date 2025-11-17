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

        // PDF Ön izleme - dosyayı inline göster
        [HttpGet("preview/{versionId:int}")]
        [Authorize]
        public async Task<IActionResult> PreviewPdf(int versionId)
        {
            try
            {
                var v = await _db.DocumentVersions.FindAsync(versionId);
                if (v is null)
                    return NotFound(new { error = "Document version not found" });

                // Check if file is PDF
                if (!v.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        error = "Only PDF files can be previewed",
                        contentType = v.ContentType,
                        message = "Please download the file to view it"
                    });
                }

                // Check authorization - owner, advisor, or admin
                var uid = GetUserId();
                var doc = await _db.Documents.FindAsync(v.DocumentId);
                if (doc == null) return NotFound();

                if (uid != doc.OwnerUserId && uid != doc.AdvisorUserId && !User.IsInRole("Admin"))
                    return Forbid();

                var stream = _storage.Open(v.StoragePath);
      
                // Return with inline disposition for browser preview
                return File(stream, "application/pdf", v.FileName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to preview document",
                    details = ex.Message
                });
            }
        }

        // Get document metadata (for PDF.js or other viewers)
        [HttpGet("metadata/{versionId:int}")]
        [Authorize]
        public async Task<IActionResult> GetDocumentMetadata(int versionId)
        {
            try
            {
                var v = await _db.DocumentVersions
                    .Include(dv => dv.Document)
                    .FirstOrDefaultAsync(dv => dv.Id == versionId);

                if (v == null)
                    return NotFound(new { error = "Document version not found" });

                // Check authorization
                var uid = GetUserId();
                if (uid != v.Document.OwnerUserId && uid != v.Document.AdvisorUserId && !User.IsInRole("Admin"))
                    return Forbid();

                return Ok(new
                {
                    v.Id,
                    v.FileName,
                    v.ContentType,
                    v.Size,
                    sizeFormatted = FormatFileSize(v.Size),
                    v.VersionNo,
                    v.CreatedAt,
                    v.Notes,
                    documentId = v.Document.Id,
                    documentTitle = v.Document.Title,
                    isPdf = v.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase),
                    canPreview = v.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase),
                    downloadUrl = $"/api/documents/download/{v.Id}",
                    previewUrl = v.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                        ? $"/api/documents/preview/{v.Id}"
                        : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to get metadata",
                    details = ex.Message
                });
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
