using Bookify.Domain.Enums;

namespace Bookify.Domain.Contracts.Booking
{
    public interface IBookingRepository
    {
        Task AddAsync(Domain.Entities.Booking booking);
        Task UpdateAsync(Domain.Entities.Booking booking);

        Task<Domain.Entities.Booking?> GetByIdAsync(Guid id);

        Task<IEnumerable<Domain.Entities.Booking>> GetByClientIdAsync(Guid clientId);
        Task<IEnumerable<Domain.Entities.Booking>> GetByStaffIdAsync(Guid staffId);

        Task<IEnumerable<Domain.Entities.Booking>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            BookingStatus? status);

        Task<bool> ExistsAsync(
            Guid staffId,
            DateTime date,
            TimeSpan time);

        Task SaveChangesAsync();
    }
}
