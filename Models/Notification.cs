namespace AdvisorySystem.Api.Models;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public string? RelatedEntityId { get; set; } // DocumentId, CommentId, etc.
    public string? RelatedEntityType { get; set; } // "Document", "Comment", "Submission"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    DeadlineApproaching,
    NewComment,
    AdvisorAssigned,
    DocumentUploaded,
    SubmissionStatusChanged,
    General
}
