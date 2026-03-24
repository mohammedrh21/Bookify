using Bookify.Domain.Enums;

namespace Bookify.Domain.Contracts.Booking
{
    public interface IBookingRepository
    {
        Task AddAsync(Domain.Entities.Booking booking);
        Task UpdateAsync(Domain.Entities.Booking booking);

        Task<Domain.Entities.Booking?> GetByIdAsync(Guid id);

        Task<IEnumerable<Domain.Entities.Booking>> GetByClientIdAsync(Guid clientId, int skip = 0, int take = 10);
        Task<IEnumerable<Domain.Entities.Booking>> GetByStaffIdAsync(Guid staffId, int skip = 0, int take = 10);
        Task<IEnumerable<Domain.Entities.Booking>> GetByStaffIdFilteredAsync(
            Guid staffId,
            BookingStatus? status = null,
            DateTime? from = null,
            DateTime? to = null,
            string? search = null,
            bool sortAscending = true,
            int skip = 0,
            int take = 10);
        Task<IEnumerable<Domain.Entities.Booking>> GetByServiceIdAsync(Guid serviceId, DateTime from, DateTime to);

        Task<IEnumerable<Domain.Entities.Booking>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            BookingStatus? status,
            string? search = null,
            string? staffNameFilter = null,
            Guid? categoryIdFilter = null,
            int skip = 0,
            int take = 10);

        Task<int> GetCountAsync(
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null,
            string? search = null,
            string? staffNameFilter = null,
            Guid? categoryIdFilter = null,
            Guid? clientId = null,
            Guid? staffId = null);

        Task<bool> ExistsAsync(
            Guid staffId,
            DateTime date,
            TimeSpan time);

        Task<int> GetCountByStatusAsync(BookingStatus status, Guid? staffId = null);
        Task<double> GetTotalRevenueAsync(Guid? staffId = null);
        
        Task SaveChangesAsync();
    }
}
