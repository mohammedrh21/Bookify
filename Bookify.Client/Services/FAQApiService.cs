using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.FAQ;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bookify.Client.Services;

public interface IFAQApiService
{
    Task<ApiResult<List<FaqModel>>> GetAllAsync();
    Task<ApiResult<FaqModel?>> GetByIdAsync(Guid id);
    Task<ApiResult<bool>> CreateAsync(FaqModel model);
    Task<ApiResult<bool>> UpdateAsync(FaqModel model);
    Task<ApiResult<bool>> DeleteAsync(Guid id);
}

public class FAQApiService(
    HttpClient http,
    ILocalStorageService localStorage,
    ToastService toast) : IFAQApiService
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

    public async Task<ApiResult<List<FaqModel>>> GetAllAsync()
    {
        var response = await http.GetAsync("api/faqs");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load FAQs.");
            ShowErrors(errors);
            return ApiResult<List<FaqModel>>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<FaqModel>>>();
        return ApiResult<List<FaqModel>>.Ok(result?.Data ?? []);
    }

    public async Task<ApiResult<FaqModel?>> GetByIdAsync(Guid id)
    {
        var response = await http.GetAsync($"api/faqs/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "FAQ not found.");
            ShowErrors(errors);
            return ApiResult<FaqModel?>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<FaqModel>>();
        return ApiResult<FaqModel?>.Ok(result?.Data);
    }

    public async Task<ApiResult<bool>> CreateAsync(FaqModel model)
    {
        await SetAuthHeaderAsync();
        var response = await http.PostAsJsonAsync("api/faqs", new { model.Question, model.Answer });
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to create FAQ.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var msg = "FAQ created successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }

    public async Task<ApiResult<bool>> UpdateAsync(FaqModel model)
    {
        await SetAuthHeaderAsync();
        var response = await http.PutAsJsonAsync($"api/faqs/{model.Id}", new { model.Id, model.Question, model.Answer });
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to update FAQ.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var msg = "FAQ updated successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }

    public async Task<ApiResult<bool>> DeleteAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await http.DeleteAsync($"api/faqs/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to delete FAQ.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var msg = "FAQ deleted successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }
}
