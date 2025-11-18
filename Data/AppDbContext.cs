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
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<StudentProfile> StudentProfiles { get; set; } = null!;
        public DbSet<CourseRequirement> CourseRequirements { get; set; } = null!;
        public DbSet<StudentCourse> StudentCourses { get; set; } = null!;
        public DbSet<DocumentRating> DocumentRatings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            
            // Title is nvarchar(max), cannot be used as index key column directly
            // If you need index on Title, change column to nvarchar(450) or similar
            
            b.Entity<DocumentVersion>()
                .HasIndex(x => new { x.DocumentId, x.VersionNo })
                .IsUnique();

            b.Entity<Comment>()
                .HasOne<DocumentVersion>()
                .WithMany()
                .HasForeignKey(c => c.DocumentVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentProfile one-to-one with AppUser
            b.Entity<StudentProfile>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // DocumentRating relationships
            b.Entity<DocumentRating>()
                .HasOne(dr => dr.DocumentVersion)
                .WithMany()
                .HasForeignKey(dr => dr.DocumentVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentCourse relationships
            b.Entity<StudentCourse>()
                .HasOne(sc => sc.CourseRequirement)
                .WithMany()
                .HasForeignKey(sc => sc.CourseRequirementId)
                .OnDelete(DeleteBehavior.Cascade);
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
        public int? DocumentId { get; set; }  // Hangi doküman için teslim isteniyor
        public Document? Document { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";  // Pending, Submitted, Late
        public DateTime? SubmittedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; } = "";  // Danışman veya Admin
    }

    public class Comment
    {
        public int Id { get; set; }
        public int DocumentVersionId { get; set; }
        public string AuthorUserId { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Öğrenci profil ve ön koşul kontrolü için yeni entity'ler
    public class StudentProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public AppUser? User { get; set; }
        public string? StudentNumber { get; set; }
        public string? Department { get; set; }
        public double? GPA { get; set; }
        public int? CompletedCredits { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public bool MeetsPrerequisites { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CourseRequirement
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = "";
        public string? CourseCode { get; set; }
        public int Credits { get; set; }
        public bool IsRequired { get; set; } = true;
        public string? Description { get; set; }
    }

    public class StudentCourse
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = "";
        public int CourseRequirementId { get; set; }
        public CourseRequirement CourseRequirement { get; set; } = default!;
        public bool IsCompleted { get; set; } = false;
        public double? Grade { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    // Danışman değerlendirme ve puanlama
    public class DocumentRating
    {
        public int Id { get; set; }
        public int DocumentVersionId { get; set; }
        public DocumentVersion DocumentVersion { get; set; } = default!;
        public string AdvisorUserId { get; set; } = "";
        public int Score { get; set; } // 1-100 arası puan
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
