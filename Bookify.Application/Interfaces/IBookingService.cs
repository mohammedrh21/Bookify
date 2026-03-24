using Bookify.Application.Common;
using Bookify.Application.DTO;
using Bookify.Application.DTO.Booking;
using Bookify.Application.DTO.Common;
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
        Task<ServiceResponse<Guid>> CancelAsync(
            CancelBookingRequest request);

        Task<ServiceResponse<Guid>> ConfirmAsync(Guid bookingId);

        Task<ServiceResponse<bool>> ConfirmCheckoutAsync(string sessionId);

        Task<ServiceResponse<Guid>> CompleteAsync(Guid bookingId);

        // =============================
        // Queries (Read Only)
        // =============================

        Task<ServiceResponse<IEnumerable<BookingResponse>>>
            GetClientBookingsAsync(Guid clientId, int page = 1, int pageSize = 10);

        Task<ServiceResponse<IEnumerable<BookingResponse>>> GetStaffBookingsAsync(Guid staffId, int page = 1, int pageSize = 10);

        Task<ServiceResponse<PagedResult<BookingResponse>>> GetStaffBookingsPagedAsync(
            Guid staffId,
            BookingStatus? status = null,
            DateTime? from = null,
            DateTime? to = null,
            string? search = null,
            bool sortAscending = true,
            int page = 1,
            int pageSize = 10);
        Task<ServiceResponse<IEnumerable<DateTime>>> GetOccupiedSlotsAsync(Guid serviceId, DateTime start, DateTime end);
        Task<ServiceResponse<PagedResult<BookingResponse>>> GetAllAsync(
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null,
            string? search = null,
            string? staffNameFilter = null,
            Guid? categoryIdFilter = null,
            int page = 1,
            int pageSize = 10);
        Task<ServiceResponse<BookingResponse>> GetByIdAsync(Guid id);

        Task<ServiceResponse<AdminDashboardResponse>> GetAdminDashboardAsync(
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<ServiceResponse<StaffDashboardResponse>> GetStaffDashboardAsync(
            Guid staffId,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
