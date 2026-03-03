
namespace Bookify.Domain.Contracts.Service
{

    public interface IServiceRepository
    {
        Task<Domain.Entities.Service?> GetByIdAsync(Guid id);
        Task<Domain.Entities.Service?> GetByStaffIdAsync(Guid staffId);
        Task<IEnumerable<Domain.Entities.Service>> GetAllAsync();
        Task AddAsync(Domain.Entities.Service service);
        Task UpdateAsync(Domain.Entities.Service service);
        Task<bool> ExistsAsync(string name, Guid staffId);
        Task SaveChangesAsync();
    }
}
