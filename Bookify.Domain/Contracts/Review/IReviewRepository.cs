namespace Bookify.Domain.Contracts.Review
{
    public interface IReviewRepository
    {
        Task AddAsync(Domain.Entities.Review review);
        Task<bool> HasClientReviewedBookingAsync(Guid bookingId);

        Task<IEnumerable<Domain.Entities.Review>> GetByServiceIdAsync(
            Guid serviceId, int skip = 0, int take = 20);

        Task<int> GetCountByServiceIdAsync(Guid serviceId);

        Task<IEnumerable<Domain.Entities.Review>> GetByClientIdAsync(
            Guid clientId, int skip = 0, int take = 20);

        Task<int> GetCountByClientIdAsync(Guid clientId);

        Task SaveChangesAsync();
    }
}
