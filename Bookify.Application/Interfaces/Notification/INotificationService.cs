using Bookify.Application.Common;
using Bookify.Application.DTO.Notification;
using Bookify.Domain.Enums;

namespace Bookify.Application.Interfaces.Notification
{
    public interface INotificationService
    {
        /// <summary>Creates a notification for a specific user.</summary>
        Task<ServiceResponse<NotificationDto>> CreateAsync(
            Guid userId,
            string title,
            string message,
            NotificationType type,
            Guid? referenceId = null,
            string? redirectUrl = null);

        /// <summary>Gets notifications for a user (optionally only unread) with pagination.</summary>
        Task<ServiceResponse<IEnumerable<NotificationDto>>> GetByUserAsync(
            Guid userId, bool unreadOnly = false, int pageNumber = 1, int pageSize = 5);

        /// <summary>Returns the count of unread notifications.</summary>
        Task<ServiceResponse<int>> GetUnreadCountAsync(Guid userId);

        /// <summary>Marks specific notifications as read.</summary>
        Task<ServiceResponse<bool>> MarkAsReadAsync(List<Guid> notificationIds);

        /// <summary>Marks all notifications for a user as read.</summary>
        Task<ServiceResponse<bool>> MarkAllAsReadAsync(Guid userId);

        /// <summary>Deletes a single notification.</summary>
        Task<ServiceResponse<bool>> DeleteAsync(Guid notificationId);

        /// <summary>Deletes notifications older than the specified number of days. Returns count deleted.</summary>
        Task<int> DeleteOlderThanAsync(int days);
    }
}
