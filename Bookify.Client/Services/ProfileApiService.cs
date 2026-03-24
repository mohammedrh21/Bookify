using Bookify.Client.Models;
using Bookify.Client.Models.Profile;
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
    {
        var result = await GetDirectAsync<ClientProfileModel>("api/profile/client", "Failed to load client profile.");
        return ApiResult<ClientProfileModel>.Ok(result.Data!);
    }

    public async Task<ApiResult<bool>> UpdateClientProfileAsync(UpdateClientProfileModel model)
        => await PutAsync("api/profile/client", model, "Failed to update client profile.");

    public async Task<ApiResult<bool>> ChangeClientPasswordAsync(ChangePasswordModel model)
        => await PostAsync("api/profile/client/change-password", model, "Failed to change password.");

    // ── Staff ─────────────────────────────────────────────────────────────

    public async Task<ApiResult<StaffProfileModel>> GetStaffProfileAsync()
    {
        var result = await GetDirectAsync<StaffProfileModel>("api/profile/staff", "Failed to load staff profile.");
        return ApiResult<StaffProfileModel>.Ok(result.Data!);
    }

    public async Task<ApiResult<bool>> UpdateStaffProfileAsync(UpdateStaffProfileModel model)
        => await PutAsync("api/profile/staff", model, "Failed to update staff profile.");

    public async Task<ApiResult<bool>> ChangeStaffPasswordAsync(ChangePasswordModel model)
        => await PostAsync("api/profile/staff/change-password", model, "Failed to change password.");

    // ── Admin ─────────────────────────────────────────────────────────────

    public async Task<ApiResult<AdminProfileModel>> GetAdminProfileAsync()
    {
        var result = await GetDirectAsync<AdminProfileModel>("api/profile/admin", "Failed to load admin profile.");
        return ApiResult<AdminProfileModel>.Ok(result.Data!);
    }

    public async Task<ApiResult<bool>> UpdateAdminProfileAsync(UpdateAdminProfileModel model)
        => await PutAsync("api/profile/admin", model, "Failed to update admin profile.");

    public async Task<ApiResult<bool>> ChangeAdminPasswordAsync(ChangePasswordModel model)
        => await PostAsync("api/profile/admin/change-password", model, "Failed to change password.");

    // ── Upload ────────────────────────────────────────────────────────────

    public async Task<ApiResult<string>> UploadProfileImageAsync(MultipartFormDataContent content, string role)
    {
        var url = role == "Staff" ? "api/profile/staff/upload-image" : "api/profile/client/upload-image";
        var response = await Http.PostAsync(url, content);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ImageUploadResult>();
            ShowSuccess("Profile image updated successfully.");
            return ApiResult<string>.Ok(result?.ImageUrl ?? string.Empty);
        }

        var errors = await ReadErrorsAsync(response, "Failed to upload image.");
        ShowErrors(errors);
        return ApiResult<string>.Fail(errors.FirstOrDefault() ?? "Error");
    }

    private class ImageUploadResult
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

