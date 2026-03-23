using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("admin")]
public class StorageController : ControllerBase
{
    private readonly IFileStorage _fileStorage;
    private readonly AppDbContext _db;
    private readonly ILogger<StorageController> _logger;
    private readonly IConfiguration _configuration;

    public StorageController(
        IFileStorage fileStorage,
     AppDbContext db,
ILogger<StorageController> logger,
      IConfiguration configuration)
    {
        _fileStorage = fileStorage;
        _db = db;
_logger = logger;
   _configuration = configuration;
    }

    // Get storage information
    [HttpGet("info")]
    public IActionResult GetStorageInfo()
    {
        var storageType = _fileStorage.GetType().Name;
   var isAzure = storageType == "AzureBlobStorage";

     var info = new
 {
     storageType = isAzure ? "Azure Blob Storage" : "Local File System",
   isProduction = isAzure,
   maxFileSize = _configuration["Storage:MaxFileSize"] ?? "104857600",
         maxFileSizeMB = (long.Parse(_configuration["Storage:MaxFileSize"] ?? "104857600") / 1024 / 1024),
            uploadPath = isAzure ? "Azure Blob Container" : _configuration["Storage:Root"]
        };

        return Ok(info);
  }

  // Get storage statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var totalFiles = await _db.DocumentVersions.CountAsync();
        var totalSize = await _db.DocumentVersions.SumAsync(v => v.Size);
     var averageSize = totalFiles > 0 ? totalSize / totalFiles : 0;

        var filesByType = await _db.DocumentVersions
         .GroupBy(v => v.ContentType)
  .Select(g => new
     {
                contentType = g.Key,
      count = g.Count(),
                totalSize = g.Sum(v => v.Size)
  })
      .OrderByDescending(x => x.count)
            .Take(10)
      .ToListAsync();

     return Ok(new
        {
  totalFiles = totalFiles,
totalSizeBytes = totalSize,
          totalSizeMB = totalSize / 1024.0 / 1024.0,
    totalSizeGB = totalSize / 1024.0 / 1024.0 / 1024.0,
            averageSizeBytes = averageSize,
       averageSizeMB = averageSize / 1024.0 / 1024.0,
      filesByType = filesByType
        });
    }

    // List all files (admin only)
  [HttpGet("files")]
    public async Task<IActionResult> ListFiles([FromQuery] string? prefix = null)
    {
 try
        {
            var files = await _fileStorage.ListAsync(prefix ?? "");
      
   // Convert to array if not already
            var filesList = files?.ToList() ?? new List<string>();
      
            return Ok(new
        {
      count = filesList.Count,
  files = filesList  // Return as array
        });
   }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files");
     return StatusCode(500, new { 
                error = "Failed to list files",
details = ex.Message 
        });
      }
    }

    // Check if file exists
    [HttpGet("exists")]
    public async Task<IActionResult> CheckFileExists([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
            return BadRequest("Path is required");

  var exists = await _fileStorage.ExistsAsync(path);
        return Ok(new { path = path, exists = exists });
    }

    // Delete orphaned files (files not referenced in database)
  [HttpDelete("cleanup-orphaned")]
    public async Task<IActionResult> CleanupOrphanedFiles()
    {
        try
    {
  var allFiles = await _fileStorage.ListAsync("");
      var dbFiles = await _db.DocumentVersions
       .Select(v => v.StoragePath)
           .ToListAsync();

 var orphanedFiles = allFiles
        .Where(f => !dbFiles.Contains(f))
        .ToList();

       var deletedCount = 0;
      foreach (var file in orphanedFiles)
       {
    try
                {
          await _fileStorage.DeleteAsync(file);
       deletedCount++;
         }
     catch (Exception ex)
  {
         _logger.LogError(ex, "Failed to delete orphaned file: {File}", file);
  }
   }

            return Ok(new
 {
                message = $"Deleted {deletedCount} orphaned files",
       totalOrphaned = orphanedFiles.Count,
              deletedCount = deletedCount,
       failed = orphanedFiles.Count - deletedCount
       });
        }
     catch (Exception ex)
        {
 _logger.LogError(ex, "Failed to cleanup orphaned files");
      return StatusCode(500, new { error = "Failed to cleanup orphaned files" });
        }
    }

    // Get file metadata
    [HttpGet("metadata/{versionId}")]
    public async Task<IActionResult> GetFileMetadata(int versionId)
    {
     var version = await _db.DocumentVersions
            .Include(v => v.Document)
    .FirstOrDefaultAsync(v => v.Id == versionId);

     if (version == null)
            return NotFound("Version not found");

        var exists = await _fileStorage.ExistsAsync(version.StoragePath);

   return Ok(new
        {
    versionId = version.Id,
     fileName = version.FileName,
     contentType = version.ContentType,
 size = version.Size,
            sizeMB = version.Size / 1024.0 / 1024.0,
      storagePath = version.StoragePath,
            exists = exists,
      uploadedBy = version.UploadedByUserId,
    uploadedAt = version.CreatedAt,
       documentId = version.DocumentId,
  documentTitle = version.Document.Title
   });
    }

    // Migrate from local to Azure (or vice versa)
    [HttpPost("migrate")]
    public async Task<IActionResult> MigrateStorage([FromBody] MigrateStorageDto dto)
 {
        // This is a placeholder for storage migration
        // Implementation would depend on having both storage services available

        return Ok(new
        {
            message = "Storage migration is not implemented in this version",
            suggestion = "Please manually copy files and update database paths"
        });
    }

    public record MigrateStorageDto(string TargetStorage);
}
