using System;
using System.Threading.Tasks;

namespace Bookify.Application.Interfaces.Notification
{
    public interface IFirebaseNotificationService
    {
        Task<bool> SendNotificationToTopicAsync(Guid userId, string title, string body, string? redirectUrl = null);
    }
}
