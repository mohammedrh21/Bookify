using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Review;
using Bookify.Application.Interfaces;
using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Contracts.Review;
using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Application.Interfaces.Auth;
using Bookify.Application.Interfaces.Notification;
using Bookify.Domain.Exceptions;

namespace Bookify.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public ReviewService(
            IReviewRepository reviewRepository, 
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository,
            IMapper mapper,
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<ServiceResponse<ReviewDto>> CreateReviewAsync(CreateReviewRequest request)
        {
            var clientId = _currentUserService.UserId ?? throw new ForbiddenException("Unauthenticated");

            // 1. Validate Booking exists and belongs to client
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);

            if (booking == null || booking.ClientId != clientId)
                throw new NotFoundException("Booking", request.BookingId);

            // 2. Ensure Booking is Completed
            if (booking.Status != BookingStatus.Completed)
                throw new BusinessRuleException("You can only rate completed bookings.");

            // 3. Check if already reviewed
            var alreadyReviewed = await _reviewRepository.HasClientReviewedBookingAsync(request.BookingId);
            
            if (alreadyReviewed)
                throw new ConflictException("This booking has already been reviewed.");

            // 4. Create Review
            var review = new Review
            {
                Id = Guid.NewGuid(),
                ServiceId = booking.ServiceId,
                ClientId = clientId,
                BookingId = request.BookingId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            await _reviewRepository.SaveChangesAsync();

            // Notify the staff member about the new review
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId);
            if (service != null)
            {
                await _notificationService.CreateAsync(
                    service.StaffId,
                    "New Review Received",
                    $"A client has left a {request.Rating}-star review on your service '{service.Name}'.",
                    Domain.Enums.NotificationType.NewReview,
                    review.Id,
                    "/staff/reviews");
            }

            var dto = _mapper.Map<ReviewDto>(review);
            return ServiceResponse<ReviewDto>.Ok(dto);
        }

        public async Task<ServiceResponse<PagedResult<ReviewDto>>> GetReviewsByServiceAsync(
            Guid serviceId, int page = 1, int pageSize = 10)
        {
            var skip  = (page - 1) * pageSize;
            var items = await _reviewRepository.GetByServiceIdAsync(serviceId, skip, pageSize);
            var total = await _reviewRepository.GetCountByServiceIdAsync(serviceId);
            var paged = new PagedResult<ReviewDto>
            {
                Items      = _mapper.Map<IEnumerable<ReviewDto>>(items),
                TotalCount = total,
                PageNumber = page,
                PageSize   = pageSize
            };
            return ServiceResponse<PagedResult<ReviewDto>>.Ok(paged);
        }

        public async Task<ServiceResponse<PagedResult<ReviewDto>>> GetReviewsByClientAsync(
            Guid clientId, int page = 1, int pageSize = 10)
        {
            if (!_currentUserService.IsAdmin && clientId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to view these reviews.");

            var skip  = (page - 1) * pageSize;
            var items = await _reviewRepository.GetByClientIdAsync(clientId, skip, pageSize);
            var total = await _reviewRepository.GetCountByClientIdAsync(clientId);
            var paged = new PagedResult<ReviewDto>
            {
                Items      = _mapper.Map<IEnumerable<ReviewDto>>(items),
                TotalCount = total,
                PageNumber = page,
                PageSize   = pageSize
            };
            return ServiceResponse<PagedResult<ReviewDto>>.Ok(paged);
        }
    }
}
