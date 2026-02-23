

namespace Bookify.Application.Interfaces.Staff
{
    public interface IStaffRepository
    {
        Task AddAsync(Domain.Entities.Staff staff);
    }
}
