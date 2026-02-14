using Bookify.Application.Common;
using Bookify.Application.DTO;
using Bookify.Application.DTO.Booking;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;


namespace Bookify.Application.Interfaces
{
    public interface IBookingService
    {
        // =============================
        // Commands (State Changes)
        // =============================

        Task<ServiceResponse<Guid>> CreateAsync(
            CreateBookingRequest request);
        Task<ServiceResponse<bool>> CancelAsync(
            CancelBookingRequest request);

        Task<ServiceResponse<bool>> ConfirmAsync(Guid bookingId);

        Task<ServiceResponse<bool>> CompleteAsync(Guid bookingId);

        // =============================
        // Queries (Read Only)
        // =============================

        Task<ServiceResponse<IEnumerable<BookingResponse>>>
            GetClientBookingsAsync(Guid clientId);

        Task<ServiceResponse<IEnumerable<BookingResponse>>>
            GetStaffBookingsAsync(Guid staffId);

        Task<ServiceResponse<IEnumerable<BookingResponse>>>
            GetAllAsync(
                DateTime? from = null,
                DateTime? to = null,
                BookingStatus? status = null);
    }
}
