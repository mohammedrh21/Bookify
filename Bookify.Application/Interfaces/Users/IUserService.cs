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

        Task<ServiceResponse<PagedResult<StaffClientDto>>> GetStaffClientsAsync(
            Guid staffId, string? search, DateTime? dateFilter, int page, int pageSize);
        Task<ServiceResponse<StaffClientDetailsDto>> GetStaffClientDetailsAsync(Guid staffId, Guid clientId);

        Task<ServiceResponse<PagedResult<AdminClientDto>>> GetAdminClientsAsync(
            string? search, int page, int pageSize);
        Task<ServiceResponse<AdminClientDetailsDto>> GetAdminClientDetailsAsync(Guid clientId);

        // Admin Staff Members
        Task<ServiceResponse<PagedResult<AdminStaffDto>>> GetAdminStaffAsync(
            string? search, int page, int pageSize);
        Task<ServiceResponse<AdminStaffDetailsDto>> GetAdminStaffDetailsAsync(Guid staffId);
    }
}
