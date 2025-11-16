using AdvisorySystem.Api.Data;
using Microsoft.EntityFrameworkCore;
using NotificationModel = AdvisorySystem.Api.Models.Notification;
using NotificationTypeEnum = AdvisorySystem.Api.Models.NotificationType;

namespace AdvisorySystem.Api.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string title, string message, NotificationTypeEnum type, 
   string? relatedEntityId = null, string? relatedEntityType = null);
    Task<List<NotificationModel>> GetUserNotificationsAsync(string userId, bool? isRead = null, int limit = 50);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
    {
        _db = db;
        _logger = logger;
  }

    public async Task CreateNotificationAsync(string userId, string title, string message, 
        NotificationTypeEnum type, string? relatedEntityId = null, string? relatedEntityType = null)
    {
    try
        {
            if (string.IsNullOrEmpty(userId))
      {
       _logger.LogWarning("Attempted to create notification with null/empty userId");
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
   }

            if (string.IsNullOrEmpty(title))
            {
        _logger.LogWarning("Attempted to create notification with null/empty title");
    throw new ArgumentException("Title cannot be null or empty", nameof(title));
          }

        if (string.IsNullOrEmpty(message))
       {
          _logger.LogWarning("Attempted to create notification with null/empty message");
     throw new ArgumentException("Message cannot be null or empty", nameof(message));
   }

     var notification = new NotificationModel
            {
     UserId = userId,
                Title = title,
        Message = message,
    Type = type,
    RelatedEntityId = relatedEntityId,
      RelatedEntityType = relatedEntityType,
     IsRead = false,
 CreatedAt = DateTime.UtcNow
       };

            _db.Notifications.Add(notification);
await _db.SaveChangesAsync();

    _logger.LogInformation("Notification created for user {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
     {
 _logger.LogError(ex, "Failed to create notification for user {UserId}: {Title}", userId, title);
  throw; // Re-throw to let caller handle
        }
    }

    public async Task<List<NotificationModel>> GetUserNotificationsAsync(string userId, bool? isRead = null, int limit = 50)
    {
   try
        {
  if (string.IsNullOrEmpty(userId))
            {
   _logger.LogWarning("GetUserNotificationsAsync called with null/empty userId");
  return new List<NotificationModel>();
            }

            var query = _db.Notifications
   .Where(n => n.UserId == userId);

      if (isRead.HasValue)
            {
        query = query.Where(n => n.IsRead == isRead.Value);
            }

      var notifications = await query
         .OrderByDescending(n => n.CreatedAt)
    .Take(limit)
         .ToListAsync();

_logger.LogDebug("Retrieved {Count} notifications for user {UserId}", notifications.Count, userId);
     return notifications;
    }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
        throw;
        }
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
   try
      {
            if (string.IsNullOrEmpty(userId))
{
      _logger.LogWarning("MarkAsReadAsync called with null/empty userId");
      return;
            }

        var notification = await _db.Notifications
       .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
       notification.IsRead = true;
      await _db.SaveChangesAsync();
    _logger.LogDebug("Marked notification {NotificationId} as read for user {UserId}", notificationId, userId);
      }
       else
       {
      _logger.LogWarning("Notification {NotificationId} not found for user {UserId}", notificationId, userId);
      }
    }
        catch (Exception ex)
        {
      _logger.LogError(ex, "Failed to mark notification {NotificationId} as read for user {UserId}", notificationId, userId);
            throw;
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
      _logger.LogWarning("MarkAllAsReadAsync called with null/empty userId");
       return;
     }

 var affectedRows = await _db.Notifications
          .Where(n => n.UserId == userId && !n.IsRead)
   .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));

   _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", affectedRows, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
   throw;
  }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
 {
        try
        {
            if (string.IsNullOrEmpty(userId))
{
            _logger.LogWarning("GetUnreadCountAsync called with null/empty userId");
              return 0;
          }

            var count = await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            _logger.LogDebug("User {UserId} has {Count} unread notifications", userId, count);
 return count;
        }
     catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to get unread count for user {UserId}", userId);
            return 0; // Return 0 instead of throwing to prevent UI breaks
        }
  }
}
