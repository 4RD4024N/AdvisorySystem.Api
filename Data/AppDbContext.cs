using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;
        public DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            
            // Title is nvarchar(max), cannot be used as index key column directly
            // If you need index on Title, change column to nvarchar(450) or similar
            
            b.Entity<DocumentVersion>()
                .HasIndex(x => new { x.DocumentId, x.VersionNo })
                .IsUnique();
        }
    }

    // Var olan entity'lerin kısaltılmış halleri
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string OwnerUserId { get; set; } = "";   // Student
        public string? AdvisorUserId { get; set; }      // eşleşmiş danışman
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    }

    public class DocumentVersion
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public Document Document { get; set; } = default!;
        public int VersionNo { get; set; }               // 1,2,3...
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long Size { get; set; }
        public string StoragePath { get; set; } = "";    // fiziksel yol
        public string UploadedByUserId { get; set; } = "";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class Submission
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = "";
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
