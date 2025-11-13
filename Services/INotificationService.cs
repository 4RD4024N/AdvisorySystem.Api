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
       _logger.LogError(ex, "Failed to create notification for user {UserId}", userId);
        }
    }

    public async Task<List<NotificationModel>> GetUserNotificationsAsync(string userId, bool? isRead = null, int limit = 50)
    {
        var query = _db.Notifications
            .Where(n => n.UserId == userId);

  if (isRead.HasValue)
        {
query = query.Where(n => n.IsRead == isRead.Value);
        }

        return await query
   .OrderByDescending(n => n.CreatedAt)
  .Take(limit)
        .ToListAsync();
    }

 public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _db.Notifications
    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
      notification.IsRead = true;
            await _db.SaveChangesAsync();
      }
 }

    public async Task MarkAllAsReadAsync(string userId)
    {
 await _db.Notifications
       .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));
 }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _db.Notifications
         .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
