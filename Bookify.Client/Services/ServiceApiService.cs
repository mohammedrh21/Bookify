using Bookify.Client.Models;
using Bookify.Client.Models.Service;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IServiceApiService
{
    Task<ApiResult<ServiceModel?>>              GetByStaffIdAsync(Guid staffId);
    Task<ApiResult<PagedResult<ServiceModel>>>  GetAllAsync(string? search = null, int page = 1, int pageSize = 10);
    Task<ApiResult<ServiceModel?>>              GetByIdAsync(Guid id);
    Task<ApiResult<bool>>                       CreateAsync(ServiceModel model);
    Task<ApiResult<bool>>                       UpdateAsync(ServiceModel model);
    Task<ApiResult<bool>>                       DeleteAsync(Guid id);
    Task<ApiResult<string>>                     UploadServiceImageAsync(Guid serviceId, MultipartFormDataContent content);
    Task<ApiResult<bool>>                       RemoveServiceImageAsync(Guid serviceId);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class ServiceApiService(HttpClient http, ToastService toast)
    : BaseApiService(http, toast), IServiceApiService
{
    // ── Queries ──────────────────────────────────────────────────────────

    public async Task<ApiResult<PagedResult<ServiceModel>>> GetAllAsync(
        string? search = null, int page = 1, int pageSize = 10)
    {
        var url = $"api/services?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        var result = await GetAsync<PagedResult<ServiceModel>>(url, "Failed to load services.");
        return ApiResult<PagedResult<ServiceModel>>.Ok(result.Data ?? new PagedResult<ServiceModel>());
    }

    public async Task<ApiResult<ServiceModel?>> GetByIdAsync(Guid id)
        => await GetAsync<ServiceModel?>($"api/services/{id}", "Service not found.");

    public async Task<ApiResult<ServiceModel?>> GetByStaffIdAsync(Guid staffId)
    {
        var response = await Http.GetAsync($"api/services/staff/{staffId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return ApiResult<ServiceModel?>.Ok(null);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load service.");
            ShowErrors(errors);
            return ApiResult<ServiceModel?>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ServiceModel>>();
        return ApiResult<ServiceModel?>.Ok(result?.Data);
    }

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task<ApiResult<bool>> CreateAsync(ServiceModel model)
        => await PostAsync("api/services", model, "Failed to create service.");

    public async Task<ApiResult<bool>> UpdateAsync(ServiceModel model)
        => await PutAsync("api/services", model, "Failed to update service.");

    public async Task<ApiResult<bool>> DeleteAsync(Guid id)
        => await DeleteAsync($"api/services/{id}", "Failed to delete service.");

    public async Task<ApiResult<string>> UploadServiceImageAsync(Guid serviceId, MultipartFormDataContent content)
    {
        var response = await Http.PostAsync($"api/services/{serviceId}/upload-image", content);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to upload service image.");
            ShowErrors(errors);
            return ApiResult<string>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ImageUploadResult>();
        ShowSuccess("Service image updated successfully.");
        return ApiResult<string>.Ok(result?.ImageUrl ?? string.Empty);
    }

    public class ImageUploadResult
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public async Task<ApiResult<bool>> RemoveServiceImageAsync(Guid serviceId)
        => await DeleteAsync($"api/services/{serviceId}/image", "Failed to remove service image.");
}

