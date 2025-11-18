using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Services;

public class DeadlineNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadlineNotificationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Her saat kontrol et

    public DeadlineNotificationService(
    IServiceProvider serviceProvider,
        ILogger<DeadlineNotificationService> logger)
{
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Deadline Notification Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
         {
      await CheckDeadlinesAsync();
      }
          catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking deadlines");
  }

        await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Deadline Notification Service stopped");
    }

    private async Task CheckDeadlinesAsync()
{
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

  var now = DateTime.UtcNow;
     var warningThreshold = now.AddDays(3); // 3 gün önceden uyar

     // Yaklaþan deadline'larý bul
        var upcomingDeadlines = await db.Submissions
     .Where(s => s.Status == "Pending" && 
          s.DueDate > now && 
                s.DueDate <= warningThreshold)
  .ToListAsync();

        foreach (var submission in upcomingDeadlines)
        {
    // Bu submission için daha önce bildirim gönderildi mi?
            var alreadyNotified = await db.Notifications
        .AnyAsync(n => n.UserId == submission.StudentId &&
      n.RelatedEntityId == submission.Id.ToString() &&
         n.RelatedEntityType == "Submission" &&
              n.Type == NotificationType.DeadlineApproaching &&
           n.CreatedAt > now.AddDays(-3)); // Son 3 gün içinde

  if (!alreadyNotified)
            {
     var daysLeft = (submission.DueDate - now).Days;
        var hoursLeft = (submission.DueDate - now).Hours;

      var message = daysLeft > 0
      ? $"Teslim tarihinize {daysLeft} gün kaldý. Tarih: {submission.DueDate:dd/MM/yyyy HH:mm}"
  : $"Teslim tarihinize {hoursLeft} saat kaldý. Tarih: {submission.DueDate:dd/MM/yyyy HH:mm}";

              var notification = new Notification
     {
        UserId = submission.StudentId,
           Title = "Teslim Tarihi Yaklaþýyor",
              Message = message,
                    Type = NotificationType.DeadlineApproaching,
    RelatedEntityId = submission.Id.ToString(),
    RelatedEntityType = "Submission",
      IsRead = false
         };

                db.Notifications.Add(notification);
     await db.SaveChangesAsync();

     _logger.LogInformation("Deadline notification sent to student {StudentId} for submission {SubmissionId}", 
     submission.StudentId, submission.Id);
            }
        }
    }
}
