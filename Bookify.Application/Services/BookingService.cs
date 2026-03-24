using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Client;
using Bookify.Application.DTO.Common;
using Bookify.Domain.Contracts;
using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Domain.Exceptions;
using Bookify.Domain.Rules;
using Bookify.Application.DTO.Review;
using Bookify.Application.Interfaces.Payment;
using Bookify.Domain.Contracts.Review;

using Bookify.Application.Interfaces.Auth;

namespace Bookify.Application.Services
{
    public sealed class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<BookingService> _logger;
        private readonly IServiceRepository _serviceRepo;
        private readonly IReviewRepository _reviewRepo;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPaymentService _paymentService;

        public BookingService(
            IBookingRepository bookingRepo,
            IMapper mapper,
            IAppLogger<BookingService> logger,
            IServiceRepository serviceRepo,
            IReviewRepository reviewRepo,
            ICurrentUserService currentUserService,
            IPaymentService paymentService)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
            _logger = logger;
            _serviceRepo = serviceRepo;
            _reviewRepo = reviewRepo;
            _currentUserService = currentUserService;
            _paymentService = paymentService;
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        /// <summary>Creates a new pending booking to lock the slot prior to Stripe Checkout.</summary>
        /// <exception cref="BusinessRuleException">When the date/time is not in the future.</exception>
        /// <exception cref="TimeSlotUnavailableException">When the slot is already taken.</exception>
        public async Task<ServiceResponse<Guid>> CreateAsync(CreateBookingRequest request)
        {
            _logger.LogInformation(
                $"Creating pending booking – Client: {request.ClientId}, Service: {request.ServiceId}, " +
                $"Date: {request.Date:yyyy-MM-dd}, Time: {request.Time}");

            if (_currentUserService.IsClient && request.ClientId != _currentUserService.UserId)
                throw new ForbiddenException("You cannot create bookings for another client.");

            if (!BookingRules.IsInFuture(request.Date, request.Time))
                throw new BusinessRuleException("Booking must be scheduled for a future date and time.");

            // Concurrency Safety (Time Slot Check)
            if (await _bookingRepo.ExistsAsync(request.StaffId, request.Date, request.Time))
            {
                throw new TimeSlotUnavailableException(request.Date, request.Time); 
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                ServiceId = request.ServiceId,
                Date = request.Date,
                Time = request.Time,
                Status = BookingStatus.Pending // Slot officially locked
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Pending Booking created: {booking.Id} awaiting Checkout.");

            return ServiceResponse<Guid>.Ok(data: booking.Id, message: "Pending booking created successfully.", id: booking.Id);
        }

        /// <summary>Confirms a pending booking securely via Stripe Checkout session ID.</summary>
        public async Task<ServiceResponse<bool>> ConfirmCheckoutAsync(string sessionId)
        {
            _logger.LogInformation($"Confirming Checkout Session: {sessionId}");

            // 1. Verify Payment Session Transaction with Stripe
            var payment = await _paymentService.VerifyCheckoutSessionStatusAsync(sessionId);

            if (payment.Status != PaymentStatus.Succeeded)
                throw new BusinessRuleException("Payment session has not succeeded. Cannot approve booking.");

            if (payment.BookingId == null)
                throw new Exception("Payment record has no linked booking.");

            // 2. Load Booking & Finalize Status
            var booking = await _bookingRepo.GetByIdAsync(payment.BookingId.Value)
                ?? throw new NotFoundException(nameof(Booking), payment.BookingId.Value);

            booking.Status = BookingStatus.Approved;
            
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Booking {booking.Id} Confirmed successfully via Stripe Session.");

            return ServiceResponse<bool>.Ok(data: true, message: "Booking checkout confirmed.");
        }

        /// <summary>Cancels a booking (client or staff may cancel).</summary>
        /// <exception cref="NotFoundException">When the booking does not exist.</exception>
        /// <exception cref="InvalidBookingTransitionException">When the booking cannot be cancelled from its current status.</exception>
        /// <exception cref="ForbiddenException">When the requester is not the owner or an authorised staff member.</exception>
        public async Task<ServiceResponse<Guid>> CancelAsync(CancelBookingRequest request)
        {
            _logger.LogInformation($"Cancelling booking: {request.BookingId}");

            var booking = await _bookingRepo.GetByIdAsync(request.BookingId)
                ?? throw new NotFoundException(nameof(Booking), request.BookingId);

            // Ownership guard
            bool isOwner = _currentUserService.IsClient && booking.ClientId == _currentUserService.UserId;
            bool isAssignedStaff = _currentUserService.IsStaff && booking.Service?.StaffId == _currentUserService.UserId;

            if (!_currentUserService.IsAdmin && !isOwner && !isAssignedStaff)
                throw new ForbiddenException("You do not have permission to cancel this booking.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new BusinessRuleException("Booking is already cancelled.");

            if (booking.Status == BookingStatus.Completed)
                throw new InvalidBookingTransitionException(
                    booking.Status.ToString(), BookingStatus.Cancelled.ToString());

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Booking cancelled: {booking.Id}");

            return ServiceResponse<Guid>.Ok(data: booking.Id, id: booking.Id, message: "Booking cancelled successfully.");
        }

        /// <summary>Confirms a pending booking (staff action).</summary>
        /// <exception cref="NotFoundException">When the booking does not exist.</exception>
        /// <exception cref="InvalidBookingTransitionException">When not in a Pending state.</exception>
        public async Task<ServiceResponse<Guid>> ConfirmAsync(Guid bookingId)
        {
            _logger.LogInformation($"Confirming booking: {bookingId}");

            var booking = await _bookingRepo.GetByIdAsync(bookingId)
                ?? throw new NotFoundException(nameof(Booking), bookingId);

            if (!_currentUserService.IsAdmin && (!_currentUserService.IsStaff || booking.Service?.StaffId != _currentUserService.UserId))
                throw new ForbiddenException("You do not have permission to confirm this booking.");

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidBookingTransitionException(
                    booking.Status.ToString(), BookingStatus.Approved.ToString());

            booking.Status = BookingStatus.Approved;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Booking confirmed: {booking.Id}");

            return ServiceResponse<Guid>.Ok(data: booking.Id, id: booking.Id, message: "Booking confirmed successfully.");
        }

        /// <summary>Marks a booking as completed (staff action).</summary>
        /// <exception cref="NotFoundException">When the booking does not exist.</exception>
        /// <exception cref="InvalidBookingTransitionException">When not in a Confirmed state.</exception>
        public async Task<ServiceResponse<Guid>> CompleteAsync(Guid bookingId)
        {
            _logger.LogInformation($"Completing booking: {bookingId}");

            var booking = await _bookingRepo.GetByIdAsync(bookingId)
                ?? throw new NotFoundException(nameof(Booking), bookingId);

            if (!_currentUserService.IsAdmin && (!_currentUserService.IsStaff || booking.Service?.StaffId != _currentUserService.UserId))
                throw new ForbiddenException("You do not have permission to complete this booking.");

            if (booking.Status != BookingStatus.Approved)
                throw new InvalidBookingTransitionException(
                    booking.Status.ToString(), BookingStatus.Completed.ToString());

            booking.Status = BookingStatus.Completed;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Booking completed: {booking.Id}");

            return ServiceResponse<Guid>.Ok(data: booking.Id, id: booking.Id, message: "Booking completed successfully.");
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetClientBookingsAsync(Guid clientId, int page = 1, int pageSize = 10)
        {
            _logger.LogInformation($"Fetching bookings for client: {clientId}");

            if (!_currentUserService.IsAdmin && clientId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to view these bookings.");

            var skip = (page - 1) * pageSize;
            var bookings = await _bookingRepo.GetByClientIdAsync(clientId, skip, pageSize);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetStaffBookingsAsync(Guid staffId, int page = 1, int pageSize = 10)
        {
            _logger.LogInformation($"Fetching bookings for staff: {staffId}");

            if (!_currentUserService.IsAdmin && staffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to view these bookings.");

            var skip = (page - 1) * pageSize;
            var bookings = await _bookingRepo.GetByStaffIdAsync(staffId, skip, pageSize);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<PagedResult<BookingResponse>>> GetStaffBookingsPagedAsync(
            Guid staffId,
            BookingStatus? status = null,
            DateTime? from = null,
            DateTime? to = null,
            string? search = null,
            bool sortAscending = true,
            int page = 1,
            int pageSize = 10)
        {
            _logger.LogInformation(
                $"Fetching paged staff bookings – Staff: {staffId}, Status: {status}, Page: {page}");

            if (!_currentUserService.IsAdmin && staffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to view these bookings.");

            if (from.HasValue && to.HasValue && from > to)
                throw new BusinessRuleException("'From' date cannot be later than 'To' date.");

            var skip = (page - 1) * pageSize;
            var bookings = await _bookingRepo.GetByStaffIdFilteredAsync(
                staffId, status, from, to, search, sortAscending, skip, pageSize);
            var total = await _bookingRepo.GetCountAsync(
                from: from, to: to, status: status, search: search, staffId: staffId);

            var paged = new PagedResult<BookingResponse>
            {
                Items = _mapper.Map<IEnumerable<BookingResponse>>(bookings),
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            };

            return ServiceResponse<PagedResult<BookingResponse>>.Ok(paged);
        }

        public async Task<ServiceResponse<IEnumerable<DateTime>>> GetOccupiedSlotsAsync(Guid serviceId, DateTime start, DateTime end)
        {
            _logger.LogInformation($"Fetching occupied slots for service: {serviceId} from {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");

            var bookings = await _bookingRepo.GetByServiceIdAsync(serviceId, start, end);
            
            var occupiedSlots = bookings.Select(b => b.Date.Date.Add(b.Time)).ToList();

            return ServiceResponse<IEnumerable<DateTime>>.Ok(occupiedSlots);
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<PagedResult<BookingResponse>>> GetAllAsync(
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null,
            string? search = null,
            string? staffNameFilter = null,
            Guid? categoryIdFilter = null,
            int page = 1,
            int pageSize = 10)
        {
            _logger.LogInformation(
                $"Fetching all bookings – From: {from}, To: {to}, Status: {status}, Page: {page}");

            if (from.HasValue && to.HasValue && from > to)
                throw new BusinessRuleException("'From' date cannot be later than 'To' date.");

            var skip = (page - 1) * pageSize;
            var bookings = await _bookingRepo.GetAllAsync(from, to, status, search, staffNameFilter, categoryIdFilter, skip, pageSize);
            var total   = await _bookingRepo.GetCountAsync(from, to, status, search, staffNameFilter, categoryIdFilter);

            var paged = new PagedResult<BookingResponse>
            {
                Items      = _mapper.Map<IEnumerable<BookingResponse>>(bookings),
                TotalCount = total,
                PageNumber = page,
                PageSize   = pageSize
            };

            return ServiceResponse<PagedResult<BookingResponse>>.Ok(paged);
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<BookingResponse>> GetByIdAsync(Guid id)
        {
            _logger.LogInformation($"Fetching booking by ID: {id}");

            var booking = await _bookingRepo.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Booking), id);

            // Ownership guard
            bool isOwner = _currentUserService.IsClient && booking.ClientId == _currentUserService.UserId;
            bool isAssignedStaff = _currentUserService.IsStaff && booking.Service?.StaffId == _currentUserService.UserId;

            if (!_currentUserService.IsAdmin && !isOwner && !isAssignedStaff)
                throw new ForbiddenException("You do not have permission to view this booking.");

            return ServiceResponse<BookingResponse>.Ok(_mapper.Map<BookingResponse>(booking));
        }
        /// <inheritdoc/>
        public async Task<ServiceResponse<AdminDashboardResponse>> GetAdminDashboardAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            _logger.LogInformation("Generating Admin dashboard stats (platform-wide).");

            // ── Periods ───────────────────────────────────────────────────────
            var end      = endDate   ?? DateTime.UtcNow;
            var start    = startDate ?? end.AddDays(-30);
            var duration = end - start;
            var prevStart = start - duration;
            var prevEnd   = start;

            // ── Fetch bookings ────────────────────────────────────────────────
            var currentBookings = (await _bookingRepo.GetAllAsync(start, end, null)).ToList();
            var prevBookings    = (await _bookingRepo.GetAllAsync(prevStart, prevEnd, null)).ToList();

            // ── 1. KPIs ───────────────────────────────────────────────────────
            var stats = new AdminDashboardResponse
            {
                TotalBookings    = currentBookings.Count,
                PendingBookings  = currentBookings.Count(b => b.Status == BookingStatus.Pending),
                ApprovedBookings = currentBookings.Count(b => b.Status == BookingStatus.Approved),
                TotalRevenue     = currentBookings
                    .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Approved)
                    .Sum(b => (double)(b.Service?.Price ?? 0)),
                TotalServices = (await _serviceRepo.GetAllAsync()).Count(s => !s.IsDeleted)
            };

            // ── 2. Trends ─────────────────────────────────────────────────────
            stats.TotalBookingsTrend = CalculateTrend(currentBookings.Count, prevBookings.Count);
            stats.PendingBookingsTrend = CalculateTrend(
                currentBookings.Count(b => b.Status == BookingStatus.Pending),
                prevBookings.Count(b  => b.Status == BookingStatus.Pending));
            stats.TotalRevenueTrend = CalculateTrend(
                stats.TotalRevenue,
                prevBookings
                    .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Approved)
                    .Sum(b => (double)(b.Service?.Price ?? 0)));

            // ── 3. Charts ─────────────────────────────────────────────────────
            var now = DateTime.UtcNow;
            for (int i = 5; i >= 0; i--)
            {
                var month      = now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd   = monthStart.AddMonths(1).AddSeconds(-1);
                var periodTotal = await _bookingRepo.GetAllAsync(monthStart, monthEnd, null);
                stats.BookingTrends.Add(new ChartDataPoint { Label = month.ToString("MMM"), Value = periodTotal.Count() });
            }

            stats.CategoryDistribution = currentBookings
                .GroupBy(b => b.Service?.Category?.Name ?? "Unknown")
                .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            // ── 4. Top Staff ──────────────────────────────────────────────────
            stats.TopStaff = currentBookings
                .GroupBy(b => new { b.Service.StaffId, b.Service.Staff.FullName, b.Service.Rating })
                .Select(g => new StaffPerformanceDto
                {
                    StaffId           = g.Key.StaffId,
                    StaffName         = g.Key.FullName,
                    CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                    Revenue           = g.Sum(b => (double)b.Service.Price),
                    Rating            = g.Key.Rating
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            // ── 5. Most Booked Services ───────────────────────────────────────
            stats.MostBookedServices = currentBookings
                .GroupBy(b => new { b.ServiceId, b.Service.Name })
                .Select(g => new ServicePerformanceDto
                {
                    ServiceId    = g.Key.ServiceId,
                    ServiceName  = g.Key.Name,
                    BookingCount = g.Count(),
                    Revenue      = g.Sum(b => (double)b.Service.Price)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToList();

            // ── 6. Recent Bookings & Activity ─────────────────────────────────
            stats.RecentBookings = _mapper.Map<List<BookingResponse>>(
                currentBookings.OrderByDescending(b => b.Date).Take(10));
            stats.RecentActivities = stats.RecentBookings.Take(5).Select(b => new RecentActivityDto
            {
                Title       = $"{b.ServiceName} Booking",
                Description = $"Client {b.ClientName} booked for {b.Date:MMM dd}",
                CreatedAt   = b.Date,
                Type        = b.Status == "Pending" ? "Warning" : "Success"
            }).ToList();

            // ── 7. Smart Insights ─────────────────────────────────────────────
            GenerateAdminInsights(stats, currentBookings, prevBookings);

            return ServiceResponse<AdminDashboardResponse>.Ok(stats);
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<StaffDashboardResponse>> GetStaffDashboardAsync(
            Guid staffId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            _logger.LogInformation($"Generating Staff dashboard stats for StaffId: {staffId}");

            if (!_currentUserService.IsAdmin && staffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to view this dashboard.");

            // ── Periods ───────────────────────────────────────────────────────
            var end      = endDate   ?? DateTime.UtcNow;
            var start    = startDate ?? end.AddDays(-30);
            var duration = end - start;
            var prevStart = start - duration;

            // ── Fetch bookings scoped to this staff member ─────────────────────
            var allCurrent = (await _bookingRepo.GetAllAsync(start, end, null)).ToList();
            var allPrev    = (await _bookingRepo.GetAllAsync(prevStart, start, null)).ToList();

            var currentBookings = allCurrent.Where(b => b.Service?.StaffId == staffId).ToList();
            var prevBookings    = allPrev.Where(b => b.Service?.StaffId == staffId).ToList();

            // ── 1. KPIs ───────────────────────────────────────────────────────
            var stats = new StaffDashboardResponse
            {
                TotalBookings    = currentBookings.Count,
                PendingBookings  = currentBookings.Count(b => b.Status == BookingStatus.Pending),
                ApprovedBookings = currentBookings.Count(b => b.Status == BookingStatus.Approved),
                TotalRevenue     = currentBookings
                    .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Approved)
                    .Sum(b => (double)(b.Service?.Price ?? 0))
            };

            // ── 2. Trends ─────────────────────────────────────────────────────
            stats.TotalBookingsTrend = CalculateTrend(currentBookings.Count, prevBookings.Count);
            stats.TotalRevenueTrend  = CalculateTrend(
                stats.TotalRevenue,
                prevBookings
                    .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Approved)
                    .Sum(b => (double)(b.Service?.Price ?? 0)));

            // ── 3. Recent Bookings & Activity ─────────────────────────────────
            stats.RecentBookings = _mapper.Map<List<BookingResponse>>(
                currentBookings.OrderByDescending(b => b.Date).Take(10));
            stats.RecentActivities = stats.RecentBookings.Take(5).Select(b => new RecentActivityDto
            {
                Title       = $"{b.ServiceName} Booking",
                Description = $"Client {b.ClientName} booked for {b.Date:MMM dd}",
                CreatedAt   = b.Date,
                Type        = b.Status == "Pending" ? "Warning" : "Success"
            }).ToList();

            // ── 4. Review Statistics ──────────────────────────────────────────
            var staffService = (await _serviceRepo.GetAllAsync())
                .FirstOrDefault(s => s.StaffId == staffId && !s.IsDeleted);

            if (staffService != null)
            {
                var allReviews  = await _reviewRepo.GetByServiceIdAsync(staffService.Id, 0, 1000);
                var reviewsList = allReviews.ToList();

                stats.TotalReviews    = reviewsList.Count;
                stats.AverageRating   = reviewsList.Count > 0 ? reviewsList.Average(r => r.Rating) : 0;
                stats.RatingDistribution = new List<int>
                {
                    reviewsList.Count(r => r.Rating == 1),
                    reviewsList.Count(r => r.Rating == 2),
                    reviewsList.Count(r => r.Rating == 3),
                    reviewsList.Count(r => r.Rating == 4),
                    reviewsList.Count(r => r.Rating == 5)
                };
                stats.RecentReviews = _mapper.Map<List<ReviewDto>>(
                    reviewsList.OrderByDescending(r => r.CreatedAt).Take(5));
            }

            return ServiceResponse<StaffDashboardResponse>.Ok(stats);
        }

        private double CalculateTrend(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }

        private void GenerateAdminInsights(AdminDashboardResponse stats, List<Booking> current, List<Booking> prev)
        {
            if (current.Count > prev.Count && prev.Count > 0)
            {
                stats.Insights.Add(new DashboardInsightDto
                {
                    Title = "Growth Detected",
                    Description = $"Bookings are up by {stats.TotalBookingsTrend:N1}% compared to the previous period.",
                    Icon = "chart-line",
                    Color = "green"
                });
            }

            var busiestDay = current.GroupBy(b => b.Date.DayOfWeek)
                                    .OrderByDescending(g => g.Count())
                                    .FirstOrDefault();
            if (busiestDay != null)
            {
                stats.Insights.Add(new DashboardInsightDto
                {
                    Title = "Peak Demand",
                    Description = $"{busiestDay.Key} is your busiest day with {busiestDay.Count()} bookings.",
                    Icon = "calendar-check",
                    Color = "blue"
                });
            }

            var topService = stats.MostBookedServices.FirstOrDefault();
            if (topService != null)
            {
                stats.Insights.Add(new DashboardInsightDto
                {
                    Title = "Service Spotlight",
                    Description = $"{topService.ServiceName} is currently your most popular service.",
                    Icon = "star",
                    Color = "yellow"
                });
            }
        }
    }
}
