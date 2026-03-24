

namespace Bookify.Application.Interfaces.Client
{
    public interface IClientRepository
    {
        Task<Domain.Entities.Client?> GetByIdAsync(Guid id);
        Task UpdateAsync(Domain.Entities.Client client);
        Task AddAsync(Domain.Entities.Client client);
        Task<(IEnumerable<Domain.Entities.Client> Items, int TotalCount)> GetClientsPaginatedAsync(int page, int pageSize);
        Task<Domain.Entities.Client?> GetClientWithBookingsAsync(Guid clientId);
    }
}
