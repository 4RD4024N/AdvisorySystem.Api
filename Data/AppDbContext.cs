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
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<CourseCategory> CourseCategories { get; set; } = null!;
        public DbSet<Prerequisite> Prerequisites { get; set; } = null!;
        public DbSet<CourseSchedule> CourseSchedules { get; set; } = null!;
        public DbSet<ScheduleConflict> ScheduleConflicts { get; set; } = null!;
        public DbSet<StudentCourseSection> StudentCourseSections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<AppUser>()
                .HasOne(u => u.Advisor)
                .WithMany()
                .HasForeignKey(u => u.AdvisorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Course>()
                .HasIndex(c => c.CourseCode)
                .IsUnique();

            b.Entity<Course>()
                .Property(c => c.CourseCode)
                .HasMaxLength(20)
                .IsUnicode(true)
                .IsRequired();

            b.Entity<Course>()
                .Property(c => c.CourseName)
                .HasMaxLength(300)
                .IsUnicode(true)
                .IsRequired();

            b.Entity<Course>()
                .Property(c => c.Description)
                .HasColumnType("nvarchar(MAX)")
                .IsUnicode(true)
                .IsRequired(false);

            b.Entity<CourseSchedule>()
                .HasIndex(cs => new { cs.CourseId, cs.Semester, cs.DayOfWeek, cs.StartTime });

            b.Entity<Prerequisite>()
                .HasOne<Course>()
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Prerequisite>()
                .HasOne<Course>()
                .WithMany()
                .HasForeignKey(p => p.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

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

    public class StudentProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public AppUser? User { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? StudentNumber { get; set; }
        public string? Department { get; set; }
        public double? GPA { get; set; }
        public int? CompletedCredits { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public bool MeetsPrerequisites { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string FullName => $"{FirstName} {LastName}".Trim();
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
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;
        public int? Semester { get; set; }
        public bool IsCompleted { get; set; } = false;
        public double? Grade { get; set; }
        public string? LetterGrade { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }

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

    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int TheoryHours { get; set; }
        public int PracticeHours { get; set; }
        public int Credits { get; set; }
        public int ECTS { get; set; }
        public int CategoryId { get; set; }
        public CourseCategory Category { get; set; } = default!;
        public int? Semester { get; set; }
        public bool IsElective { get; set; } = false;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TotalWeeklyHours => TheoryHours + PracticeHours;
    }

    public class CourseCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class Prerequisite
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int PrerequisiteCourseId { get; set; }
        public bool IsMandatory { get; set; } = true;
    }

    public class CourseSchedule
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;
        public int Semester { get; set; }
        public string SectionCode { get; set; } = "A"; // A, B, C, D...
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
        public string? InstructorName { get; set; }
        public bool IsTheory { get; set; } = true;
        public int SessionNumber { get; set; } = 1; // 1, 2, 3, 4 (for multi-session courses)
        public int MaxCapacity { get; set; } = 50;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class StudentCourseSection
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = "";
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;
        public string SectionCode { get; set; } = "A";
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;
    }

    public class ScheduleConflict
    {
        public int Id { get; set; }
        public int Schedule1Id { get; set; }
        public int Schedule2Id { get; set; }
        public string ConflictType { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }
}
