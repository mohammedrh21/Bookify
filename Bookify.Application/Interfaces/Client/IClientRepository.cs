

namespace Bookify.Application.Interfaces.Client
{
    public interface IClientRepository
    {
        Task<Domain.Entities.Client?> GetByIdAsync(Guid id);
        Task UpdateAsync(Domain.Entities.Client client);
        Task AddAsync(Domain.Entities.Client client);
        Task<(IEnumerable<Domain.Entities.Client> Items, int TotalCount)> GetClientsPaginatedAsync(int page, int pageSize);
        Task<Domain.Entities.Client?> GetClientWithBookingsAsync(Guid clientId);
        Task<(IEnumerable<Bookify.Application.DTO.Users.StaffClientDto> Items, int TotalCount)> GetStaffClientsPaginatedAsync(
            Guid staffId, string? search, DateTime? dateFilter, int page, int pageSize);
        Task<Bookify.Application.DTO.Users.StaffClientDetailsDto?> GetStaffClientDetailsAsync(Guid staffId, Guid clientId);
        Task<(IEnumerable<Bookify.Application.DTO.Users.AdminClientDto> Items, int TotalCount)> GetAdminClientsPaginatedAsync(
            string? search, int page, int pageSize);
        Task<Bookify.Application.DTO.Users.AdminClientDetailsDto?> GetAdminClientDetailsAsync(Guid clientId);
    }
}
