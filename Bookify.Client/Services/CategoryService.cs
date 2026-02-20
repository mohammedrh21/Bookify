using Blazored.LocalStorage;
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
    Task<bool> DeleteAsync(Guid id);
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
        return await http.GetFromJsonAsync<List<CategoryModel>>("api/categories") ?? [];
    }

    public async Task<CategoryModel?> GetByIdAsync(Guid id)
    {
        await SetAuthHeader();
        return await http.GetFromJsonAsync<CategoryModel>($"api/categories/{id}");
    }

    public async Task<bool> CreateAsync(CategoryModel model)
    {
        await SetAuthHeader();
        var response = await http.PostAsJsonAsync("api/categories", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.DeleteAsync($"api/categories/{id}");
        return response.IsSuccessStatusCode;
    }
}
