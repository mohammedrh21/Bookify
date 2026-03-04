using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.SupportTicket;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bookify.Client.Services;

public interface ITicketApiService
{
    Task<ApiResult<List<TicketModel>>> GetAllAsync();
    Task<ApiResult<bool>> SubmitAsync(TicketModel model);
}

public class TicketApiService(
    HttpClient http,
    ILocalStorageService localStorage,
    ToastService toast) : ITicketApiService
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

    public async Task<ApiResult<List<TicketModel>>> GetAllAsync()
    {
        await SetAuthHeaderAsync(); // Admin only
        var response = await http.GetAsync("api/tickets");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load support tickets.");
            ShowErrors(errors);
            return ApiResult<List<TicketModel>>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketModel>>>();
        return ApiResult<List<TicketModel>>.Ok(result?.Data ?? []);
    }

    public async Task<ApiResult<bool>> SubmitAsync(TicketModel model)
    {
        var response = await http.PostAsJsonAsync("api/tickets", new { model.Email, model.Subject, model.Description });
        
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var msg = "You have already submitted a support ticket today. Please try again tomorrow.";
            toast.ShowError(msg);
            return ApiResult<bool>.Fail(msg);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to submit ticket.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        
        return ApiResult<bool>.Ok(true, "Ticket submitted successfully.");
    }
}
