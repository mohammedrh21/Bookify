using Bookify.Client.Models;
using Bookify.Client.Models.Category;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface ICategoryService
{
    Task<ApiResult<List<CategoryModel>>>  GetAllAsync(int? limit = null);
    Task<ApiResult<CategoryModel?>>       GetByIdAsync(Guid id);
    Task<ApiResult<bool>>                 CreateAsync(CategoryModel model);
    Task<ApiResult<bool>>                 DeactivateAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class CategoryService(HttpClient http, ToastService toast)
    : BaseApiService(http, toast), ICategoryService
{
    // ── Queries ──────────────────────────────────────────────────────────

    public async Task<ApiResult<List<CategoryModel>>> GetAllAsync(int? limit = null)
    {
        var url = limit.HasValue ? $"api/categories?limit={limit.Value}" : "api/categories";
        var result = await GetAsync<List<CategoryModel>>(url, "Failed to load categories.");
        return ApiResult<List<CategoryModel>>.Ok(result.Data ?? []);
    }

    public async Task<ApiResult<CategoryModel?>> GetByIdAsync(Guid id)
        => await GetAsync<CategoryModel?>($"api/categories/{id}", "Category not found.");

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task<ApiResult<bool>> CreateAsync(CategoryModel model)
        => await PostAsync("api/categories", new { model.Name }, "Failed to create category.");

    /// <summary>Soft-deletes a category via PATCH /api/categories/{id}/deactivate.</summary>
    public async Task<ApiResult<bool>> DeactivateAsync(Guid id)
        => await PatchAsync($"api/categories/{id}/deactivate", (object)null!, "Failed to deactivate category.");
}

