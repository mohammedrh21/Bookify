using Bookify.Client.Models;
using Bookify.Client.Models.Common;
using Bookify.Client.Models.Notification;

namespace Bookify.Client.Services
{
    public interface INotificationApiService
    {
        Task<ApiResult<IEnumerable<NotificationModel>?>> GetNotificationsAsync(int pageNumber = 1, int pageSize = 5, bool unreadOnly = false);
        Task<ApiResult<int>> GetUnreadCountAsync();
        Task<ApiResult<bool>> MarkAsReadAsync(List<Guid> notificationIds);
        Task<ApiResult<bool>> MarkAllAsReadAsync();
        Task<ApiResult<bool>> DeleteNotificationAsync(Guid id);
    }

    public class NotificationApiService(HttpClient http, ToastService toast)
        : BaseApiService(http, toast), INotificationApiService
    {
        public async Task<ApiResult<IEnumerable<NotificationModel>?>> GetNotificationsAsync(int pageNumber = 1, int pageSize = 5, bool unreadOnly = false)
        {
            var url = $"api/notifications?unreadOnly={unreadOnly}&pageNumber={pageNumber}&pageSize={pageSize}";
            return await GetAsync<IEnumerable<NotificationModel>>(url, "Failed to fetch notifications.");
        }

        public async Task<ApiResult<int>> GetUnreadCountAsync()
            => await GetAsync<int>(
                "api/notifications/unread-count",
                "Failed to fetch unread count.");

        public async Task<ApiResult<bool>> MarkAsReadAsync(List<Guid> notificationIds)
            => await PostAsync(
                "api/notifications/mark-read",
                new MarkNotificationsReadRequest { NotificationIds = notificationIds },
                "Failed to mark notifications as read.");

        public async Task<ApiResult<bool>> MarkAllAsReadAsync()
            => await PostAsync<object>(
                "api/notifications/mark-all-read",
                new { },
                "Failed to mark all notifications as read.");

        public async Task<ApiResult<bool>> DeleteNotificationAsync(Guid id)
            => await DeleteAsync(
                $"api/notifications/{id}",
                "Failed to delete notification.");
    }
}
