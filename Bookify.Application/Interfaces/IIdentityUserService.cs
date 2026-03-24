using Bookify.Application.Common;
using Bookify.Application.DTO.Identity;
using Microsoft.AspNetCore.Http;

namespace Bookify.Application.Interfaces
{
    public interface IIdentityUserService
    {
        Task<ServiceResponse<string>> CreateUserAsync(
            string email,
            string password,
            string phone,
            string role);

        Task<ServiceResponse<ClientProfileResponse>> GetClientProfileAsync(string userId);
        Task<ServiceResponse<bool>> UpdateClientProfileAsync(string userId, UpdateClientProfileRequest request);

        Task<ServiceResponse<StaffProfileResponse>> GetStaffProfileAsync(string userId);
        Task<ServiceResponse<bool>> UpdateStaffProfileAsync(string userId, UpdateStaffProfileRequest request);

        Task<ServiceResponse<AdminProfileResponse>> GetAdminProfileAsync(string userId);
        Task<ServiceResponse<bool>> UpdateAdminProfileAsync(string userId, UpdateAdminProfileRequest request);

        Task<ServiceResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<ServiceResponse<bool>> ToggleUserActiveAsync(Guid userId);
        Task<ServiceResponse<string>> UpdateProfileImageAsync(string userId, IFormFile file, string role);
    }
}
