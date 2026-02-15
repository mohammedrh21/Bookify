using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO;
using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Domain.Rules;

namespace Bookify.Application.Services
{
    public sealed class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepo,
            IMapper mapper,
            IAppLogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        // =============================
        // Commands
        // =============================

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateBookingRequest request)
        {
            try
            {
                // FIXED: Inverted logic - booking MUST be in the future
                if (!BookingRules.IsInFuture(request.Date, request.Time))
                {
                    _logger.LogWarning($"Attempted to create booking in the past: {request.Date} {request.Time}");
                    return ServiceResponse<Guid>.Fail("Booking must be for a future date/time");
                }

                // Check if the time slot is already booked
                if (await _bookingRepo.ExistsAsync(request.StaffId, request.Date, request.Time))
                {
                    _logger.LogWarning($"Time slot already booked: Staff {request.StaffId}, {request.Date} {request.Time}");
                    return ServiceResponse<Guid>.Fail("Time slot already booked");
                }

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

                _logger.LogInformation($"Booking created successfully: {booking.Id}");

                return ServiceResponse<Guid>.Ok(
                    id: booking.Id,
                    message: "Booking created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return ServiceResponse<Guid>.Fail("An error occurred while creating the booking");
            }
        }

        public async Task<ServiceResponse<bool>> CancelAsync(CancelBookingRequest request)
        {
            try
            {
                var booking = await _bookingRepo.GetByIdAsync(request.BookingId);

                if (booking is null)
                {
                    _logger.LogWarning($"Booking not found: {request.BookingId}");
                    return ServiceResponse<bool>.Fail("Booking not found");
                }

                if (!BookingRules.CanCancel(booking.Status))
                {
                    _logger.LogWarning($"Cannot cancel booking {request.BookingId} with status {booking.Status}");
                    return ServiceResponse<bool>.Fail($"Booking cannot be cancelled. Current status: {booking.Status}");
                }

                // Validate requester authorization
                if (request.RequesterType == BookingRequesterType.Client &&
                    booking.ClientId != request.RequesterId)
                {
                    _logger.LogWarning($"Unauthorized cancellation attempt by client {request.RequesterId} for booking {request.BookingId}");
                    return ServiceResponse<bool>.Fail("You are not authorized to cancel this booking");
                }

                booking.Status = BookingStatus.Cancelled;
                await _bookingRepo.UpdateAsync(booking);
                await _bookingRepo.SaveChangesAsync();

                _logger.LogInformation($"Booking cancelled: {booking.Id} by {request.RequesterType}");

                return ServiceResponse<bool>.Ok(true, "Booking cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling booking {request.BookingId}");
                return ServiceResponse<bool>.Fail("An error occurred while cancelling the booking");
            }
        }

        public async Task<ServiceResponse<bool>> ConfirmAsync(Guid bookingId)
        {
            try
            {
                var booking = await _bookingRepo.GetByIdAsync(bookingId);

                if (booking is null)
                {
                    _logger.LogWarning($"Booking not found: {bookingId}");
                    return ServiceResponse<bool>.Fail("Booking not found");
                }

                if (!BookingRules.CanConfirm(booking.Status))
                {
                    _logger.LogWarning($"Cannot confirm booking {bookingId} with status {booking.Status}");
                    return ServiceResponse<bool>.Fail($"Booking cannot be confirmed. Current status: {booking.Status}");
                }

                booking.Status = BookingStatus.Approved;
                await _bookingRepo.UpdateAsync(booking);
                await _bookingRepo.SaveChangesAsync();

                _logger.LogInformation($"Booking confirmed: {booking.Id}");

                return ServiceResponse<bool>.Ok(true, "Booking confirmed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming booking {bookingId}");
                return ServiceResponse<bool>.Fail("An error occurred while confirming the booking");
            }
        }

        public async Task<ServiceResponse<bool>> CompleteAsync(Guid bookingId)
        {
            try
            {
                var booking = await _bookingRepo.GetByIdAsync(bookingId);

                if (booking is null)
                {
                    _logger.LogWarning($"Booking not found: {bookingId}");
                    return ServiceResponse<bool>.Fail("Booking not found");
                }

                if (!BookingRules.CanComplete(booking.Status))
                {
                    _logger.LogWarning($"Cannot complete booking {bookingId} with status {booking.Status}");
                    return ServiceResponse<bool>.Fail($"Booking cannot be completed. Current status: {booking.Status}");
                }

                booking.Status = BookingStatus.Completed;
                await _bookingRepo.UpdateAsync(booking);
                await _bookingRepo.SaveChangesAsync();

                _logger.LogInformation($"Booking completed: {booking.Id}");

                return ServiceResponse<bool>.Ok(true, "Booking completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing booking {bookingId}");
                return ServiceResponse<bool>.Fail("An error occurred while completing the booking");
            }
        }

        // =============================
        // Queries
        // =============================

        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetClientBookingsAsync(Guid clientId)
        {
            try
            {
                var bookings = await _bookingRepo.GetByClientIdAsync(clientId);
                var response = _mapper.Map<IEnumerable<BookingResponse>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponse>>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for client {clientId}");
                return ServiceResponse<IEnumerable<BookingResponse>>.Fail("An error occurred while retrieving bookings");
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetStaffBookingsAsync(Guid staffId)
        {
            try
            {
                var bookings = await _bookingRepo.GetByStaffIdAsync(staffId);
                var response = _mapper.Map<IEnumerable<BookingResponse>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponse>>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for staff {staffId}");
                return ServiceResponse<IEnumerable<BookingResponse>>.Fail("An error occurred while retrieving bookings");
            }
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            BookingStatus? status)
        {
            try
            {
                var bookings = await _bookingRepo.GetAllAsync(from, to, status);
                var response = _mapper.Map<IEnumerable<BookingResponse>>(bookings);

                return ServiceResponse<IEnumerable<BookingResponse>>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all bookings");
                return ServiceResponse<IEnumerable<BookingResponse>>.Fail("An error occurred while retrieving bookings");
            }
        }
    }
}
