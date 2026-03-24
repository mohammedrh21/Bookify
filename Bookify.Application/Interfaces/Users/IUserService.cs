using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Users;

namespace Bookify.Application.Interfaces.Users
{
    public interface IUserService
    {
        Task<PagedResult<StaffDto>> GetStaffPaginatedAsync(int page, int pageSize);
        Task<PagedResult<ClientDto>> GetClientsPaginatedAsync(int page, int pageSize);
        Task<ServiceResponse<bool>> ToggleUserActiveAsync(Guid id);
        Task<ServiceResponse<ClientReportDto>> GetClientReportAsync(Guid id);
    }
}
