using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Notification;
using Bookify.Application.Interfaces.Notification;
using Bookify.Domain.Contracts;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;

namespace Bookify.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IMapper _mapper;
        private readonly IFirebaseNotificationService _firebaseNotificationService;

        public NotificationService(
            INotificationRepository repo, 
            IMapper mapper,
            IFirebaseNotificationService firebaseNotificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _firebaseNotificationService = firebaseNotificationService;
        }

        public async Task<ServiceResponse<NotificationDto>> CreateAsync(
            Guid userId,
            string title,
            string message,
            NotificationType type,
            Guid? referenceId = null,
            string? redirectUrl = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ReferenceId = referenceId,
                RedirectUrl = redirectUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(notification);

            // Push to Firebase via background-like fire-and-forget or awaited without failing the main transaction
            _ = Task.Run(() => _firebaseNotificationService.SendNotificationToTopicAsync(userId, title, message, redirectUrl));

            var dto = _mapper.Map<NotificationDto>(notification);
            return ServiceResponse<NotificationDto>.Ok(dto);
        }

        public async Task<ServiceResponse<IEnumerable<NotificationDto>>> GetByUserAsync(
            Guid userId, bool unreadOnly = false, int pageNumber = 1, int pageSize = 5)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50; // Cap to prevent massive loads

            var notifications = await _repo.GetByUserIdAsync(userId, unreadOnly, pageNumber, pageSize);
            var dtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
            return ServiceResponse<IEnumerable<NotificationDto>>.Ok(dtos);
        }

        public async Task<ServiceResponse<int>> GetUnreadCountAsync(Guid userId)
        {
            var count = await _repo.GetUnreadCountAsync(userId);
            return ServiceResponse<int>.Ok(count);
        }

        public async Task<ServiceResponse<bool>> MarkAsReadAsync(List<Guid> notificationIds)
        {
            await _repo.MarkAsReadAsync(notificationIds);
            return ServiceResponse<bool>.Ok(true, "Notifications marked as read.");
        }

        public async Task<ServiceResponse<bool>> MarkAllAsReadAsync(Guid userId)
        {
            await _repo.MarkAllAsReadAsync(userId);
            return ServiceResponse<bool>.Ok(true, "All notifications marked as read.");
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(Guid notificationId)
        {
            var notification = await _repo.GetByIdAsync(notificationId);
            if (notification == null)
                return ServiceResponse<bool>.Fail("Notification not found.");

            await _repo.DeleteAsync(notificationId);

            return ServiceResponse<bool>.Ok(true, "Notification deleted.");
        }

        public async Task<int> DeleteOlderThanAsync(int days)
        {
            return await _repo.DeleteOlderThanAsync(days);
        }
    }
}
