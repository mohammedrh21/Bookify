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
using Bookify.Application.Interfaces.Notification;
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
        private readonly INotificationService _notificationService;

        public BookingService(
            IBookingRepository bookingRepo,
            IMapper mapper,
            IAppLogger<BookingService> logger,
            IServiceRepository serviceRepo,
            IReviewRepository reviewRepo,
            ICurrentUserService currentUserService,
            IPaymentService paymentService,
            INotificationService notificationService)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
            _logger = logger;
            _serviceRepo = serviceRepo;
            _reviewRepo = reviewRepo;
            _currentUserService = currentUserService;
            _paymentService = paymentService;
            _notificationService = notificationService;
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

            // Notify the Staff who owns the service
            var service = await _serviceRepo.GetByIdAsync(request.ServiceId);
            if (service?.StaffId != null)
            {
                await _notificationService.CreateAsync(
                    service.StaffId,
                    "New Booking Received",
                    $"A client has booked your service '{service.Name}' for {request.Date:MMM dd, yyyy}.",
                    NotificationType.NewBooking,
                    booking.Id,
                    "/booking/service-bookings");
            }

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

            // Notify the other party about cancellation
            if (_currentUserService.IsStaff || _currentUserService.IsAdmin)
            {
                await _notificationService.CreateAsync(
                    booking.ClientId,
                    "Booking Cancelled",
                    $"Your booking for '{booking.Service?.Name}' on {booking.Date:MMM dd, yyyy} has been cancelled.",
                    NotificationType.BookingStatusChanged,
                    booking.Id,
                    "/my-bookings");
            }
            else if (_currentUserService.IsClient && booking.Service?.StaffId != null)
            {
                await _notificationService.CreateAsync(
                    booking.Service.StaffId,
                    "Booking Cancelled by Client",
                    $"A client cancelled the booking for '{booking.Service?.Name}' on {booking.Date:MMM dd, yyyy}.",
                    NotificationType.BookingStatusChanged,
                    booking.Id,
                    "/booking/service-bookings");
            }

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

            // Notify the client that their booking is confirmed
            await _notificationService.CreateAsync(
                booking.ClientId,
                "Booking Confirmed",
                $"Your booking for '{booking.Service?.Name}' on {booking.Date:MMM dd, yyyy} has been confirmed.",
                NotificationType.BookingStatusChanged,
                booking.Id,
                "/my-bookings");

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

            // Notify the client that their booking is completed
            await _notificationService.CreateAsync(
                booking.ClientId,
                "Booking Completed",
                $"Your booking for '{booking.Service?.Name}' on {booking.Date:MMM dd, yyyy} has been completed. Feel free to leave a review!",
                NotificationType.BookingStatusChanged,
                booking.Id,
                "/my-bookings");

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
            // Normalize to Utc: .Date strips DateTimeKind, so SpecifyKind is required
            // before passing any DateTime to PostgreSQL 'timestamp with time zone' columns.
            var end      = UtcDate(endDate   ?? DateTime.UtcNow);
            var start    = UtcDate(startDate ?? end.AddDays(-30));
            var durationDays = (end - start).TotalDays + 1;
            var prevStart = start.AddDays(-durationDays);
            var prevEnd   = start.AddDays(-1);

            // ── Fetch bookings ────────────────────────────────────────────────
            var currentBookings = (await _bookingRepo.GetAdminDashboardBookingsAsync(start, end)).ToList();
            var prevBookings    = (await _bookingRepo.GetAdminDashboardBookingsAsync(prevStart, prevEnd)).ToList();

            // ── Fetch ALL bookings (for all-time KPIs) has been replaced by optimized DB queries ──

            // ── 1. KPIs (ALL-TIME, platform-wide) ─────────────────────────────
            var allServices = (await _serviceRepo.GetAllAsync()).ToList();
            var stats = new AdminDashboardResponse
            {
                TotalBookings    = await _bookingRepo.GetCountAsync(),
                PendingBookings  = await _bookingRepo.GetCountByStatusAsync(BookingStatus.Pending),
                ApprovedBookings = await _bookingRepo.GetCountByStatusAsync(BookingStatus.Approved),
                TotalRevenue     = await _bookingRepo.GetTotalPlatformRevenueAsync(),
                TotalServices    = allServices.Count(s => !s.IsDeleted && s.IsActive)
            };

            // ── 2. Trends ─────────────────────────────────────────────────────
            stats.TotalBookingsTrend = CalculateTrend(currentBookings.Count, prevBookings.Count);
            stats.PendingBookingsTrend = CalculateTrend(
                currentBookings.Count(b => b.Status == BookingStatus.Pending),
                prevBookings.Count(b  => b.Status == BookingStatus.Pending));
            stats.TotalRevenueTrend = CalculateTrend(
                currentBookings
                    .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded)
                    .Sum(b => (double)b.Payment!.Amount),
                prevBookings
                    .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded)
                    .Sum(b => (double)b.Payment!.Amount));

            // ── 3. Charts ─────────────────────────────────────────────────────
            var now = DateTime.UtcNow;
            for (int i = 5; i >= 0; i--)
            {
                var month      = now.AddMonths(-i);
                var monthStart = DateTime.SpecifyKind(new DateTime(month.Year, month.Month, 1), DateTimeKind.Utc);
                var monthEnd   = monthStart.AddMonths(1).AddSeconds(-1);
                var periodTotal = await _bookingRepo.GetAdminDashboardBookingsAsync(monthStart, monthEnd);
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
                .Where(b => b.Service?.Staff != null)
                .GroupBy(b => new { b.Service.StaffId, b.Service.Staff.FullName, b.Service.Rating })
                .Select(g => new StaffPerformanceDto
                {
                    StaffId           = g.Key.StaffId,
                    StaffName         = g.Key.FullName,
                    CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                    Revenue           = g.Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded).Sum(b => (double)b.Payment!.Amount),
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
                    Revenue      = g.Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded).Sum(b => (double)b.Payment!.Amount)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToList();

            // ── 6. Recent Bookings & Activity (all-time most recent 10) ────────
            var latestBookingsAllTime = await _bookingRepo.GetAllAsync(null, null, null, take: 10);
            stats.RecentBookings = _mapper.Map<List<BookingResponse>>(latestBookingsAllTime);
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

        public async Task<ServiceResponse<StaffDashboardResponse>> GetStaffDashboardAsync(
            Guid staffId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            _logger.LogInformation($"Generating Staff dashboard stats for StaffId: {staffId}");

            if (!_currentUserService.IsAdmin && staffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to view this dashboard.");

            // ── Periods ───────────────────────────────────────────────────────
            // Normalize to Utc: .Date strips DateTimeKind, so SpecifyKind is required
            // before passing any DateTime to PostgreSQL 'timestamp with time zone' columns.
            var end      = UtcDate(endDate   ?? DateTime.UtcNow);
            var start    = UtcDate(startDate ?? end.AddDays(-30));

            var durationDays = (end - start).TotalDays + 1;
            var prevStart = start.AddDays(-durationDays);
            var prevEnd   = start.AddDays(-1);

            // ── Fetch bookings scoped to this staff member ─────────────────────
            var currentBookings = (await _bookingRepo.GetStaffDashboardBookingsAsync(staffId, start, end)).ToList();
            var prevBookings    = (await _bookingRepo.GetStaffDashboardBookingsAsync(staffId, prevStart, prevEnd)).ToList();

            // DIAGNOSTIC LOGS:
            var allBookings = (await _bookingRepo.GetByStaffIdAsync(staffId, 0, 1000)).ToList();
            var bookingDetails = string.Join(", ", allBookings.Select(b => $"{b.Date:yyyy-MM-dd} ({b.Status})"));
            _logger.LogInformation(
                $"Dashboard Diagnostics [StaffId: {staffId}]: " +
                $"Current Period: {currentBookings.Count}, " +
                $"Previous Period: {prevBookings.Count}, " +
                $"All-Time Count: {allBookings.Count}, " +
                $"Details: [{bookingDetails}], " +
                $"Filter Range: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");

            // ── 1. KPIs ───────────────────────────────────────────────────────
            var stats = new StaffDashboardResponse
            {
                TotalBookings    = currentBookings.Count,
                PendingBookings  = currentBookings.Count(b => b.Status == BookingStatus.Pending),
                ApprovedBookings = currentBookings.Count(b => b.Status == BookingStatus.Approved),
                TotalRevenue     = currentBookings
                    .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded)
                    .Sum(b => (double)b.Payment!.Amount),
                ActiveClients    = allBookings.Select(b => b.ClientId).Distinct().Count(),
                CompletionRate   = currentBookings.Count > 0 
                    ? Math.Round((double)currentBookings.Count(b => b.Status == BookingStatus.Completed) / currentBookings.Count * 100, 1) 
                    : 0,
                CancellationRate = currentBookings.Count > 0 
                    ? Math.Round((double)currentBookings.Count(b => b.Status == BookingStatus.Cancelled) / currentBookings.Count * 100, 1) 
                    : 0
            };

            // ── 2. Trends ─────────────────────────────────────────────────────
            stats.TotalBookingsTrend = CalculateTrend(currentBookings.Count, prevBookings.Count);
            stats.TotalRevenueTrend  = CalculateTrend(
                stats.TotalRevenue,
                prevBookings
                    .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded)
                    .Sum(b => (double)b.Payment!.Amount));

            // ── 3. Charts ─────────────────────────────────────────────────────
            // Partition data based on the chosen range
            var totalDays = (end.Date - start.Date).TotalDays + 1;

            if (totalDays <= 1.1) // Day
            {
                stats.BookingTrends.Add(new ChartDataPoint { Label = "Morning", Value = currentBookings.Count(b => b.Time.Hours < 12) });
                stats.BookingTrends.Add(new ChartDataPoint { Label = "Afternoon", Value = currentBookings.Count(b => b.Time.Hours >= 12 && b.Time.Hours < 17) });
                stats.BookingTrends.Add(new ChartDataPoint { Label = "Evening", Value = currentBookings.Count(b => b.Time.Hours >= 17) });
            }
            else if (totalDays <= 10) // Week (usually 7 days)
            {
                for (int i = 0; i < totalDays; i++)
                {
                    var date = start.AddDays(i);
                    var count = currentBookings.Count(b => b.Date.Date == date.Date);
                    stats.BookingTrends.Add(new ChartDataPoint { Label = date.ToString("ddd"), Value = count });
                }
            }
            else if (totalDays >= 28 && totalDays <= 31) // Month
            {
                // Custom 5-week breakdown: [1-7, 8-14, 15-21, 22-28, 29-end]
                int[][] dayRanges = new int[][] {
                    new[] {1, 7}, new[] {8, 14}, new[] {15, 21}, new[] {22, 28}, new[] {29, 31}
                };

                for (int i = 0; i < dayRanges.Length; i++)
                {
                    int startDay = dayRanges[i][0];
                    int endDay = i == 4 ? DateTime.DaysInMonth(start.Year, start.Month) : dayRanges[i][1];
                    
                    if (startDay > endDay) continue;

                    var count = currentBookings.Count(b => b.Date.Day >= startDay && b.Date.Day <= endDay);
                    stats.BookingTrends.Add(new ChartDataPoint { Label = $"Week {i + 1}", Value = count });
                }
            }
            else if (totalDays > 300) // Year
            {
                for (int m = 1; m <= 12; m++)
                {
                    var count = currentBookings.Count(b => b.Date.Month == m);
                    stats.BookingTrends.Add(new ChartDataPoint 
                    { 
                        Label = new DateTime(start.Year, m, 1).ToString("MMM"), 
                        Value = count 
                    });
                }
            }
            else // Fallback for other ranges
            {
                var pointCount = 6;
                var intervalTicks = (end.Ticks - start.Ticks) / pointCount;
                for (int i = 0; i < pointCount; i++)
                {
                    var pStart = start.AddTicks(intervalTicks * i);
                    var pEnd = i == pointCount - 1 ? end : pStart.AddTicks(intervalTicks);
                    var count = currentBookings.Count(b => b.Date.Date >= pStart.Date && b.Date.Date <= pEnd.Date);
                    stats.BookingTrends.Add(new ChartDataPoint { Label = pStart.ToString("MMM dd"), Value = count });
                }
            }

            // ── 4. Recent & Upcoming (Pending + Approved only) ─────────────────
            var recentUpcoming = allBookings
                .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved)
                .OrderByDescending(b => b.Date)
                .Take(10)
                .ToList();
            stats.RecentBookings = _mapper.Map<List<BookingResponse>>(recentUpcoming);
            stats.RecentActivities = stats.RecentBookings.Take(5).Select(b => new RecentActivityDto
            {
                Title       = $"{b.ServiceName} Booking",
                Description = $"Client {b.ClientName} booked for {b.Date:MMM dd}",
                CreatedAt   = b.Date,
                Type        = b.Status == "Pending" ? "Warning" : "Success"
            }).ToList();

            // ── 5. Review Statistics ──────────────────────────────────────────
            var staffService = await _serviceRepo.GetByStaffIdAsync(staffId);

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

        /// <summary>
        /// Returns midnight of the given date with <see cref="DateTimeKind.Utc"/> explicitly set.
        /// Necessary because <see cref="DateTime.Date"/> always strips the Kind to Unspecified,
        /// which Npgsql 6+ rejects for 'timestamp with time zone' columns.
        /// </summary>
        private static DateTime UtcDate(DateTime dt) =>
            DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);

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
