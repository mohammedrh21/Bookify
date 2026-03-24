

namespace Bookify.Application.Interfaces.Staff
{
    public interface IStaffRepository
    {
        Task<Domain.Entities.Staff?> GetByIdAsync(Guid id);
        Task UpdateAsync(Domain.Entities.Staff staff);
        Task AddAsync(Domain.Entities.Staff staff);
        Task<(IEnumerable<Domain.Entities.Staff> Items, int TotalCount)> GetStaffPaginatedAsync(int page, int pageSize);
    }
}
