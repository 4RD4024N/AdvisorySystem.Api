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
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    private string? GetUserId()
    {
        try
        {
          // Try multiple claim types
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
       ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
   ?? User.FindFirstValue("sub")
        ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
    ?? User.Identity?.Name;

         if (!string.IsNullOrEmpty(userId))
            {
         _logger.LogDebug("User ID found: {UserId}", userId);
       return userId;
            }

// Log all available claims for debugging
    var claims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
            _logger.LogWarning("User ID not found. Available claims: {Claims}", claims);
            
    return null;
        }
      catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from claims");
    return null;
        }
    }

    // Get my notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool? isRead = null, [FromQuery] int limit = 50)
 {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
       _logger.LogWarning("GetMyNotifications: User ID is null or empty");
         return Unauthorized(new { error = "User identification failed", message = "Unable to identify user from token" });
            }

 var notifications = await _notificationService.GetUserNotificationsAsync(userId, isRead, limit);
 return Ok(notifications);
        }
        catch (Exception ex)
  {
            _logger.LogError(ex, "Failed to get notifications");
   return StatusCode(500, new { 
    error = "Failed to retrieve notifications", 
       message = ex.Message,
    innerError = ex.InnerException?.Message 
  });
        }
    }

    // Get unread count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
  {
           _logger.LogWarning("GetUnreadCount: User ID is null or empty");
    // Return 0 instead of error for better UX
   return Ok(new { unreadCount = 0 });
            }

       var count = await _notificationService.GetUnreadCountAsync(userId);
         return Ok(new { unreadCount = count });
}
     catch (Exception ex)
        {
  _logger.LogError(ex, "Failed to get unread count");
            // Return 0 instead of 500 for better UX
        return Ok(new { unreadCount = 0 });
        }
}

    // Mark notification as read
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
    try
        {
 var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized(new { error = "User identification failed" });
 }

   await _notificationService.MarkAsReadAsync(id, userId);
        return Ok(new { message = "Notification marked as read" });
        }
        catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to mark notification as read: {Id}", id);
            return StatusCode(500, new { error = "Failed to mark notification as read" });
        }
    }

    // Mark all as read
    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
  try
        {
     var userId = GetUserId();
         if (string.IsNullOrEmpty(userId))
          {
             return Unauthorized(new { error = "User identification failed" });
            }

            await _notificationService.MarkAllAsReadAsync(userId);
 return Ok(new { message = "All notifications marked as read" });
  }
    catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read");
          return StatusCode(500, new { error = "Failed to mark all notifications as read" });
        }
    }

    // Create notification (Admin only - for testing)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
  {
        try
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
        catch (Exception ex)
      {
            _logger.LogError(ex, "Failed to create notification");
        return StatusCode(500, new { error = "Failed to create notification" });
        }
    }

    // Test endpoint to check user claims (Development only)
    [HttpGet("test-claims")]
    #if DEBUG
    [AllowAnonymous]
    #else
    [Authorize(Roles = "Admin")]
    #endif
    public IActionResult TestClaims()
    {
   var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var userId = GetUserId();
        
        return Ok(new
      {
 userId = userId,
    isAuthenticated = User.Identity?.IsAuthenticated,
         authenticationType = User.Identity?.AuthenticationType,
      name = User.Identity?.Name,
            claims = claims
        });
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
