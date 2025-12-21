using Microsoft.AspNetCore.Identity;

namespace AdvisorySystem.Api.Models
{
    public class AppUser : IdentityUser
    {
        // İstersen extra alanlar: StudentNo, FullName vs.
        
        // Öğrenci ise, atanan öğretmen (advisor)
        public string? AdvisorId { get; set; }

        // Navigation property
        public virtual AppUser? Advisor { get; set; }
    }
}
