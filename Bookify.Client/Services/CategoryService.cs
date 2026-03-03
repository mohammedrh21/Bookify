using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.Category;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface ICategoryService
{
    Task<ApiResult<List<CategoryModel>>> GetAllAsync();
    Task<ApiResult<CategoryModel?>>      GetByIdAsync(Guid id);
    Task<ApiResult<bool>>               CreateAsync(CategoryModel model);
    Task<ApiResult<bool>>               DeactivateAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class CategoryService(
    HttpClient http,
    ILocalStorageService localStorage,
    ToastService toast) : ICategoryService
{
    private async Task SetAuthHeaderAsync()
    {
        var token = await localStorage.GetItemAsync<string>("access_token");
        http.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
            ? new AuthenticationHeaderValue("Bearer", token)
            : null;
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

    public async Task<ApiResult<List<CategoryModel>>> GetAllAsync()
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync("api/categories");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load categories.");
            ShowErrors(errors);
            return ApiResult<List<CategoryModel>>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryModel>>>();
        return ApiResult<List<CategoryModel>>.Ok(result?.Data ?? []);
    }

    public async Task<ApiResult<CategoryModel?>> GetByIdAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync($"api/categories/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Category not found.");
            ShowErrors(errors);
            return ApiResult<CategoryModel?>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryModel>>();
        return ApiResult<CategoryModel?>.Ok(result?.Data);
    }

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task<ApiResult<bool>> CreateAsync(CategoryModel model)
    {
        await SetAuthHeaderAsync();
        var response = await http.PostAsJsonAsync("api/categories", new { model.Name });
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to create category.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Category created successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }

    /// <summary>Soft-deletes a category via PATCH /api/categories/{id}/deactivate.</summary>
    public async Task<ApiResult<bool>> DeactivateAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await http.PatchAsync($"api/categories/{id}/deactivate", null);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to deactivate category.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Category deactivated successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }
}
