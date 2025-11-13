using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvisorsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _users;
    private readonly INotificationService _notificationService;

    public AdvisorsController(AppDbContext db, UserManager<AppUser> users, INotificationService notificationService)
    {
        _db = db;
        _users = users;
        _notificationService = notificationService;
    }

    // Tüm danýþmanlarý listele
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
   var advisors = await _users.GetUsersInRoleAsync("Advisor");
        var result = advisors.Select(a => new
        {
  a.Id,
       a.UserName,
   a.Email
  });
  return Ok(result);
    }

    // Öðrenciye danýþman ata
    [HttpPost("assign")]
    [Authorize(Roles = "Admin,Advisor")]
    public async Task<IActionResult> AssignAdvisor([FromBody] AssignAdvisorDto dto)
    {
        var doc = await _db.Documents.FindAsync(dto.DocumentId);
 if (doc == null) return NotFound("Document not found");

     var advisor = await _users.FindByIdAsync(dto.AdvisorUserId);
        if (advisor == null || !await _users.IsInRoleAsync(advisor, "Advisor"))
            return BadRequest("Invalid advisor");

        doc.AdvisorUserId = dto.AdvisorUserId;
        await _db.SaveChangesAsync();

        // Notify student
await _notificationService.CreateNotificationAsync(
  doc.OwnerUserId,
   "Advisor Assigned",
    $"An advisor has been assigned to your document: {doc.Title}",
            NotificationType.AdvisorAssigned,
  doc.Id.ToString(),
            "Document"
        );

   // Notify advisor
await _notificationService.CreateNotificationAsync(
      dto.AdvisorUserId,
       "New Assignment",
     $"You have been assigned as advisor for document: {doc.Title}",
  NotificationType.AdvisorAssigned,
   doc.Id.ToString(),
    "Document"
        );

        return Ok(new { message = "Advisor assigned successfully" });
 }

    public record AssignAdvisorDto(int DocumentId, string AdvisorUserId);
}
