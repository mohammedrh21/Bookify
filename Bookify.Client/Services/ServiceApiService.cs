using Blazored.LocalStorage;
using Bookify.Client.Models.Service;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IServiceApiService
{
    Task<List<ServiceModel>> GetAllAsync();
    Task<List<ServiceModel>> GetByCategoryAsync(Guid categoryId);
    Task<ServiceModel?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(ServiceModel model);
    Task<bool> UpdateAsync(ServiceModel model);
    Task<bool> DeleteAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class ServiceApiService(HttpClient http, ILocalStorageService localStorage)
    : IServiceApiService
{
    private async Task SetAuthHeader()
    {
        var token = await localStorage.GetItemAsync<string>("access_token");
        if (!string.IsNullOrEmpty(token))
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<ServiceModel>> GetAllAsync()
    {
        await SetAuthHeader();
        return await http.GetFromJsonAsync<List<ServiceModel>>("api/services") ?? [];
    }

    public async Task<List<ServiceModel>> GetByCategoryAsync(Guid categoryId)
    {
        await SetAuthHeader();
        return await http.GetFromJsonAsync<List<ServiceModel>>(
            $"api/services?categoryId={categoryId}") ?? [];
    }

    public async Task<ServiceModel?> GetByIdAsync(Guid id)
    {
        await SetAuthHeader();
        return await http.GetFromJsonAsync<ServiceModel>($"api/services/{id}");
    }

    public async Task<bool> CreateAsync(ServiceModel model)
    {
        await SetAuthHeader();
        var response = await http.PostAsJsonAsync("api/services", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(ServiceModel model)
    {
        await SetAuthHeader();
        var response = await http.PutAsJsonAsync($"api/services/{model.Id}", model);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.DeleteAsync($"api/services/{id}");
        return response.IsSuccessStatusCode;
    }
}
