using Bookify.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Domain.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = default!;

        public NotificationType Type { get; set; } = NotificationType.General;

        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Optional reference to a related entity (e.g. BookingId, ServiceId).
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>
        /// Client-side redirect URL when the notification is clicked.
        /// </summary>
        [MaxLength(300)]
        public string? RedirectUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
