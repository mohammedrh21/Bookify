using Bookify.Client.Models;
using Bookify.Client.Models.Profile;
using Bookify.Client.Models.Common;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

public interface IProfileApiService
{
    // Client
    Task<ApiResult<ClientProfileModel>> GetClientProfileAsync();
    Task<ApiResult<bool>> UpdateClientProfileAsync(UpdateClientProfileModel model);
    Task<ApiResult<bool>> ChangeClientPasswordAsync(ChangePasswordModel model);

    // Staff
    Task<ApiResult<StaffProfileModel>> GetStaffProfileAsync();
    Task<ApiResult<bool>> UpdateStaffProfileAsync(UpdateStaffProfileModel model);
    Task<ApiResult<bool>> ChangeStaffPasswordAsync(ChangePasswordModel model);

    Task<ApiResult<string>> UploadProfileImageAsync(MultipartFormDataContent content, string role);

    // Admin
    Task<ApiResult<AdminProfileModel>> GetAdminProfileAsync();
    Task<ApiResult<bool>> UpdateAdminProfileAsync(UpdateAdminProfileModel model);
    Task<ApiResult<bool>> ChangeAdminPasswordAsync(ChangePasswordModel model);
}

public class ProfileApiService(HttpClient http, ToastService toast)
    : BaseApiService(http, toast), IProfileApiService
{
    // ── Client ────────────────────────────────────────────────────────────

    public async Task<ApiResult<ClientProfileModel>> GetClientProfileAsync()
        => (await GetAsync<ClientProfileModel>("api/profile/client", "Failed to load client profile."))!;

    public async Task<ApiResult<bool>> UpdateClientProfileAsync(UpdateClientProfileModel model)
        => await PutAsync("api/profile/client", model, "Failed to update client profile.");

    public async Task<ApiResult<bool>> ChangeClientPasswordAsync(ChangePasswordModel model)
        => await PostDirectAsync("api/profile/client/change-password", model, "Failed to change password.");

    // ── Staff ─────────────────────────────────────────────────────────────

    public async Task<ApiResult<StaffProfileModel>> GetStaffProfileAsync()
        => (await GetAsync<StaffProfileModel>("api/profile/staff", "Failed to load staff profile."))!;

    public async Task<ApiResult<bool>> UpdateStaffProfileAsync(UpdateStaffProfileModel model)
        => await PutAsync("api/profile/staff", model, "Failed to update staff profile.");

    public async Task<ApiResult<bool>> ChangeStaffPasswordAsync(ChangePasswordModel model)
        => await PostDirectAsync("api/profile/staff/change-password", model, "Failed to change password.");

    // ── Admin ─────────────────────────────────────────────────────────────

    public async Task<ApiResult<AdminProfileModel>> GetAdminProfileAsync()
        => (await GetAsync<AdminProfileModel>("api/profile/admin", "Failed to load admin profile."))!;

    public async Task<ApiResult<bool>> UpdateAdminProfileAsync(UpdateAdminProfileModel model)
        => await PutAsync("api/profile/admin", model, "Failed to update admin profile.");

    public async Task<ApiResult<bool>> ChangeAdminPasswordAsync(ChangePasswordModel model)
        => await PostDirectAsync("api/profile/admin/change-password", model, "Failed to change password.");

    // ── Upload ────────────────────────────────────────────────────────────

    public async Task<ApiResult<string>> UploadProfileImageAsync(MultipartFormDataContent content, string role)
    {
        var url = role == "Staff" ? "api/profile/staff/upload-image" : "api/profile/client/upload-image";
        var response = await Http.PostAsync(url, content);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            ShowSuccess("Profile image updated successfully.");
            return ApiResult<string>.Ok(result?.Data ?? string.Empty);
        }

        var errors = await ReadErrorsAsync(response, "Failed to upload image.");
        ShowErrors(errors);
        return ApiResult<string>.Fail(errors.FirstOrDefault() ?? "Error");
    }

}

