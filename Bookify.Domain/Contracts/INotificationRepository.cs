using Bookify.Domain.Entities;

namespace Bookify.Domain.Contracts
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool unreadOnly, int pageNumber = 1, int pageSize = 5);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(List<Guid> notificationIds);
        Task MarkAllAsReadAsync(Guid userId);
        Task<int> DeleteOlderThanAsync(int days);
    }
}
