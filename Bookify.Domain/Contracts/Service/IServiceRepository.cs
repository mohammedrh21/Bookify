
namespace Bookify.Domain.Contracts.Service
{

    public interface IServiceRepository
    {
        Task<Domain.Entities.Service?> GetByIdAsync(Guid id);
        Task<Domain.Entities.Service?> GetByStaffIdAsync(Guid staffId);
        Task<IEnumerable<Domain.Entities.Service>> GetAllAsync(string? searchTerm = null, int skip = 0, int take = 10);
        Task<int> GetCountAsync(string? searchTerm = null);
        Task AddAsync(Domain.Entities.Service service);
        Task UpdateAsync(Domain.Entities.Service service);
        Task RemoveAsync(Domain.Entities.Service service);
        Task<bool> ExistsAsync(string name, Guid staffId);
        Task SaveChangesAsync();
    }
}
