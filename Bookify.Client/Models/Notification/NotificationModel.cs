namespace Bookify.Client.Models.Notification
{
    public class NotificationModel
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

    public class MarkNotificationsReadRequest
    {
        public List<Guid> NotificationIds { get; set; } = new();
    }
}
