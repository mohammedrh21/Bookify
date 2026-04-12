namespace Bookify.Application.DTO.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Type { get; set; } = default!;
        public bool IsRead { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? RedirectUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationRequest
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Type { get; set; } = "General";
        public Guid? ReferenceId { get; set; }
        public string? RedirectUrl { get; set; }
    }

    public class MarkNotificationsReadRequest
    {
        public List<Guid> NotificationIds { get; set; } = new();
    }
}
