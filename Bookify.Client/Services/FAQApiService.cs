using Bookify.Client.Models;
using Bookify.Client.Models.FAQ;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

public interface IFAQApiService
{
    Task<ApiResult<PagedResult<FaqModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null);
    Task<ApiResult<FaqModel?>>             GetByIdAsync(Guid id);
    Task<ApiResult<bool>>                  CreateAsync(FaqModel model);
    Task<ApiResult<bool>>                  UpdateAsync(FaqModel model);
    Task<ApiResult<bool>>                  DeleteAsync(Guid id);
}

public class FAQApiService(HttpClient http, ToastService toast)
    : BaseApiService(http, toast), IFAQApiService
{
    public async Task<ApiResult<PagedResult<FaqModel>>> GetAllAsync(
        int pageNumber = 1, int pageSize = 10, string? search = null)
    {
        var url = $"api/faqs?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var result = await GetAsync<PagedResult<FaqModel>>(url, "Failed to load FAQs.");
        return ApiResult<PagedResult<FaqModel>>.Ok(result.Data);
    }

    public async Task<ApiResult<FaqModel?>> GetByIdAsync(Guid id)
        => await GetAsync<FaqModel?>($"api/faqs/{id}", "FAQ not found.");

    public async Task<ApiResult<bool>> CreateAsync(FaqModel model)
        => await PostAsync("api/faqs", new { model.Question, model.Answer }, "Failed to create FAQ.");

    public async Task<ApiResult<bool>> UpdateAsync(FaqModel model)
        => await PutAsync($"api/faqs/{model.Id}", new { model.Id, model.Question, model.Answer }, "Failed to update FAQ.");

    public async Task<ApiResult<bool>> DeleteAsync(Guid id)
        => await DeleteAsync($"api/faqs/{id}", "Failed to delete FAQ.");
}

