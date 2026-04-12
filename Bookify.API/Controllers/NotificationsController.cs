using Bookify.Application.DTO.Notification;
using Bookify.Application.Interfaces.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>Get current user's notifications with pagination.</summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var result = await _notificationService.GetByUserAsync(CurrentUserGuid, unreadOnly, pageNumber, pageSize);
            return HandleResult(result);
        }

        /// <summary>Get unread notification count.</summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await _notificationService.GetUnreadCountAsync(CurrentUserGuid);
            return HandleResult(result);
        }

        /// <summary>Mark specific notifications as read.</summary>
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkNotificationsReadRequest request)
        {
            var result = await _notificationService.MarkAsReadAsync(request.NotificationIds);
            return HandleResult(result);
        }

        /// <summary>Mark all notifications as read.</summary>
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _notificationService.MarkAllAsReadAsync(CurrentUserGuid);
            return HandleResult(result);
        }

        /// <summary>Delete a notification.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _notificationService.DeleteAsync(id);
            return HandleResult(result);
        }
    }
}
