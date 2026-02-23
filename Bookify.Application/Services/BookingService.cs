using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Client;
using Bookify.Domain.Contracts;
using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Domain.Exceptions;
using Bookify.Domain.Rules;

namespace Bookify.Application.Services
{
    /// <summary>
    /// Application service for managing <see cref="Booking"/> entities.
    /// All business-rule violations throw typed <see cref="DomainException"/>s
    /// that are handled uniformly by <c>GlobalExceptionMiddleware</c>.
    /// </summary>
    public sealed class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<BookingService> _logger;
        public BookingService(
            IBookingRepository bookingRepo,
            IMapper mapper,
            IAppLogger<BookingService> logger,
            IGenericRepository<Client> genericRepo)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        /// <summary>Creates a new booking for a client.</summary>
        /// <exception cref="BusinessRuleException">When the date/time is not in the future.</exception>
        /// <exception cref="TimeSlotUnavailableException">When the slot is already taken.</exception>
        public async Task<ServiceResponse<Guid>> CreateAsync(CreateBookingRequest request)
        {
            _logger.LogInformation(
                $"Creating booking – Client: {request.ClientId}, Service: {request.ServiceId}, " +
                $"Date: {request.Date:yyyy-MM-dd}, Time: {request.Time}");

            if (!BookingRules.IsInFuture(request.Date, request.Time))
                throw new BusinessRuleException("Booking must be scheduled for a future date and time.");

            if (await _bookingRepo.ExistsAsync(request.StaffId, request.Date, request.Time))
                throw new TimeSlotUnavailableException(request.Date, request.Time);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                ServiceId = request.ServiceId,
                Date = request.Date,
                Time = request.Time,
                Status = BookingStatus.Pending
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Booking created: {booking.Id}");

            return ServiceResponse<Guid>.Ok(booking.Id, "Booking created successfully.");
        }

        /// <summary>Cancels a booking (client or staff may cancel).</summary>
        /// <exception cref="NotFoundException">When the booking does not exist.</exception>
        /// <exception cref="InvalidBookingTransitionException">When the booking cannot be cancelled from its current status.</exception>
        /// <exception cref="ForbiddenException">When the requester is not the owner or an authorised staff member.</exception>
        public async Task<ServiceResponse<bool>> CancelAsync(CancelBookingRequest request)
        {
            _logger.LogInformation($"Cancelling booking: {request.BookingId}");

            var booking = await _bookingRepo.GetByIdAsync(request.BookingId)
                ?? throw new NotFoundException(nameof(Booking), request.BookingId);

            // Ownership guard
            bool isOwner = request.RequesterType == BookingRequesterType.Client
                           && booking.ClientId == request.RequesterId;
            bool isAssignedStaff = request.RequesterType == BookingRequesterType.Staff
                                   && booking.Service?.StaffId == request.RequesterId;

            if (!isOwner && !isAssignedStaff)
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

            return ServiceResponse<bool>.Ok(true, "Booking cancelled successfully.");
        }

        /// <summary>Confirms a pending booking (staff action).</summary>
        /// <exception cref="NotFoundException">When the booking does not exist.</exception>
        /// <exception cref="InvalidBookingTransitionException">When not in a Pending state.</exception>
        public async Task<ServiceResponse<bool>> ConfirmAsync(Guid bookingId)
        {
            _logger.LogInformation($"Confirming booking: {bookingId}");

            var booking = await _bookingRepo.GetByIdAsync(bookingId)
                ?? throw new NotFoundException(nameof(Booking), bookingId);

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidBookingTransitionException(
                    booking.Status.ToString(), BookingStatus.Approved.ToString());

            booking.Status = BookingStatus.Approved;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Booking confirmed: {booking.Id}");

            return ServiceResponse<bool>.Ok(true, "Booking confirmed successfully.");
        }

        /// <summary>Marks a booking as completed (staff action).</summary>
        /// <exception cref="NotFoundException">When the booking does not exist.</exception>
        /// <exception cref="InvalidBookingTransitionException">When not in a Confirmed state.</exception>
        public async Task<ServiceResponse<bool>> CompleteAsync(Guid bookingId)
        {
            _logger.LogInformation($"Completing booking: {bookingId}");

            var booking = await _bookingRepo.GetByIdAsync(bookingId)
                ?? throw new NotFoundException(nameof(Booking), bookingId);

            if (booking.Status != BookingStatus.Approved)
                throw new InvalidBookingTransitionException(
                    booking.Status.ToString(), BookingStatus.Completed.ToString());

            booking.Status = BookingStatus.Completed;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            _logger.LogInformation($"Booking completed: {booking.Id}");

            return ServiceResponse<bool>.Ok(true, "Booking completed successfully.");
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetClientBookingsAsync(Guid clientId)
        {
            _logger.LogInformation($"Fetching bookings for client: {clientId}");

            var bookings = await _bookingRepo.GetByClientIdAsync(clientId);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetStaffBookingsAsync(Guid staffId)
        {
            _logger.LogInformation($"Fetching bookings for staff: {staffId}");

            var bookings = await _bookingRepo.GetByStaffIdAsync(staffId);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetAllAsync(
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null)
        {
            _logger.LogInformation(
                $"Fetching all bookings – From: {from}, To: {to}, Status: {status}");

            if (from.HasValue && to.HasValue && from > to)
                throw new BusinessRuleException("'From' date cannot be later than 'To' date.");

            var bookings = await _bookingRepo.GetAllAsync(from, to, status);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }
    }
}
