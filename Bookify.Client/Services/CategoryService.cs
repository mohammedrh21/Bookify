using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.Category;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface ICategoryService
{
    Task<List<CategoryModel>> GetAllAsync();
    Task<CategoryModel?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(CategoryModel model);
    Task<bool> DeactivateAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class CategoryService(HttpClient http, ILocalStorageService localStorage)
    : ICategoryService
{
    private async Task SetAuthHeader()
    {
        var token = await localStorage.GetItemAsync<string>("access_token");
        if (!string.IsNullOrEmpty(token))
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<CategoryModel>> GetAllAsync()
    {
        await SetAuthHeader();
        var response = await http.GetFromJsonAsync<ApiResponse<List<CategoryModel>>>("api/categories");
        return response?.Data ?? [];
    }

    public async Task<CategoryModel?> GetByIdAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.GetFromJsonAsync<ApiResponse<CategoryModel>>($"api/categories/{id}");
        return response?.Data;
    }

    public async Task<bool> CreateAsync(CategoryModel model)
    {
        await SetAuthHeader();
        var response = await http.PostAsJsonAsync("api/categories", new { model.Name });
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Uses PATCH /api/categories/{id}/deactivate (the API's soft-delete).
    /// </summary>
    public async Task<bool> DeactivateAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.PatchAsync($"api/categories/{id}/deactivate", null);
        return response.IsSuccessStatusCode;
    }
}
