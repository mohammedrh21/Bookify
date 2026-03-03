using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.Service;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IServiceApiService
{
    Task<ApiResult<ServiceModel?>>       GetByStaffIdAsync(Guid staffId);
    Task<ApiResult<List<ServiceModel>>>  GetAllAsync();
    Task<ApiResult<ServiceModel?>>       GetByIdAsync(Guid id);
    Task<ApiResult<bool>>               CreateAsync(ServiceModel model);
    Task<ApiResult<bool>>               UpdateAsync(ServiceModel model);
    Task<ApiResult<bool>>               DeleteAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class ServiceApiService(
    HttpClient http,
    ILocalStorageService localStorage,
    ToastService toast) : IServiceApiService
{
    private async Task SetAuthHeaderAsync()
    {
        var token = await localStorage.GetItemAsync<string>("access_token");
        http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<List<string>> ReadErrorsAsync(HttpResponseMessage response, string fallback)
    {
        var errors = new List<string>();
        try
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            // 1. Check for RFC 9110 validation errors dict
            if (json.TryGetProperty("errors", out var errorsDict) && errorsDict.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errorsDict.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var errStr in prop.Value.EnumerateArray())
                        {
                            if (errStr.ValueKind == JsonValueKind.String)
                            {
                                var val = errStr.GetString();
                                if (!string.IsNullOrWhiteSpace(val)) errors.Add(val);
                            }
                        }
                    }
                }
            }

            // 2. Fallback to standard message property
            if (errors.Count == 0 && json.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
            {
                var val = msg.GetString();
                if (!string.IsNullOrWhiteSpace(val)) errors.Add(val);
            }
        }
        catch { }

        if (errors.Count == 0) errors.Add(fallback);
        return errors;
    }

    private void ShowErrors(List<string> errors)
    {
        foreach (var error in errors)
        {
            toast.ShowError(error);
        }
    }

    // ── Queries ──────────────────────────────────────────────────────────

    public async Task<ApiResult<List<ServiceModel>>> GetAllAsync()
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync("api/services");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load services.");
            ShowErrors(errors);
            return ApiResult<List<ServiceModel>>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ServiceModel>>>();
        return ApiResult<List<ServiceModel>>.Ok(result?.Data ?? []);
    }

    public async Task<ApiResult<ServiceModel?>> GetByIdAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync($"api/services/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Service not found.");
            ShowErrors(errors);
            return ApiResult<ServiceModel?>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ServiceModel>>();
        return ApiResult<ServiceModel?>.Ok(result?.Data);
    }

    public async Task<ApiResult<ServiceModel?>> GetByStaffIdAsync(Guid staffId)
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync($"api/services/staff/{staffId}");

        // 404 means the staff member simply has no service – not an error, no toast
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
    {
        await SetAuthHeaderAsync();
        var response = await http.PostAsJsonAsync("api/services", model);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to create service.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Service created successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }

    public async Task<ApiResult<bool>> UpdateAsync(ServiceModel model)
    {
        await SetAuthHeaderAsync();
        var response = await http.PutAsJsonAsync("api/services", model);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to update service.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Service updated successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }

    public async Task<ApiResult<bool>> DeleteAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await http.DeleteAsync($"api/services/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to delete service.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Service deleted successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }
}
