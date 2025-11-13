using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
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

    // Get my notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool? isRead = null, [FromQuery] int limit = 50)
    {
        var userId = GetUserId();
   var notifications = await _notificationService.GetUserNotificationsAsync(userId, isRead, limit);
  return Ok(notifications);
    }

    // Get unread count
    [HttpGet("unread-count")]
 public async Task<IActionResult> GetUnreadCount()
    {
   var userId = GetUserId();
     var count = await _notificationService.GetUnreadCountAsync(userId);
  return Ok(new { unreadCount = count });
    }

    // Mark notification as read
 [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
      var userId = GetUserId();
     await _notificationService.MarkAsReadAsync(id, userId);
   return Ok(new { message = "Notification marked as read" });
    }

    // Mark all as read
 [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
   var userId = GetUserId();
    await _notificationService.MarkAllAsReadAsync(userId);
return Ok(new { message = "All notifications marked as read" });
    }

    // Create notification (Admin only - for testing)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
    {
  await _notificationService.CreateNotificationAsync(
            dto.UserId,
         dto.Title,
   dto.Message,
 dto.Type,
            dto.RelatedEntityId,
            dto.RelatedEntityType
        );
        return Ok(new { message = "Notification created" });
    }

    public record CreateNotificationDto(
        string UserId,
        string Title,
   string Message,
  NotificationType Type,
     string? RelatedEntityId,
        string? RelatedEntityType
    );
}
