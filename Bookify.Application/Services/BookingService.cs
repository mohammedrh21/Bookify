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

        public BookingService(
            IBookingRepository bookingRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        // =============================
        // Commands
        // =============================

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateBookingRequest request)
        {
            if (BookingRules.IsInFuture(request.Date,request.Time))
                return ServiceResponse<Guid>.Fail("Booking must be for a future date/time");

            if (await _bookingRepo.ExistsAsync(
                request.StaffId,
                request.Date,
                request.Time))
                return ServiceResponse<Guid>.Fail("Time slot already booked");

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

            return ServiceResponse<Guid>.Ok(
               id:booking.Id,
               message: "Booking created");
        }

        public async Task<ServiceResponse<bool>> CancelAsync(CancelBookingRequest request)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);

            if (booking is null)
                return ServiceResponse<bool>.Fail("Booking not found");

            if (!BookingRules.CanCancel(booking.Status))
                return ServiceResponse<bool>.Fail("Booking cannot be cancelled");

            if (request.RequesterType == BookingRequesterType.Client &&
                booking.ClientId != request.RequesterId)
                return ServiceResponse<bool>.Fail("Unauthorized cancellation");

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Booking cancelled");
        }

        public async Task<ServiceResponse<bool>> ConfirmAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            if (booking is null)
                return ServiceResponse<bool>.Fail("Booking not found");

            if (!BookingRules.CanConfirm(booking.Status))
                return ServiceResponse<bool>.Fail("Booking cannot be confirmed");

            booking.Status = BookingStatus.Approved;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Booking confirmed");
        }

        public async Task<ServiceResponse<bool>> CompleteAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            if (booking is null)
                return ServiceResponse<bool>.Fail("Booking not found");

            if (!BookingRules.CanComplete(booking.Status))
                return ServiceResponse<bool>.Fail("Booking cannot be completed");

            booking.Status = BookingStatus.Completed;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Booking completed");
        }

        // =============================
        // Queries
        // =============================

        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetClientBookingsAsync(Guid clientId)
        {
            var bookings = await _bookingRepo.GetByClientIdAsync(clientId);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetStaffBookingsAsync(Guid staffId)
        {
            var bookings = await _bookingRepo.GetByStaffIdAsync(staffId);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }

        public async Task<ServiceResponse<IEnumerable<BookingResponse>>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            BookingStatus? status)
        {
            var bookings = await _bookingRepo.GetAllAsync(from, to, status);
            return ServiceResponse<IEnumerable<BookingResponse>>.Ok(
                _mapper.Map<IEnumerable<BookingResponse>>(bookings));
        }
    }
}
