using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.Booking;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IBookingService
{
    Task<ApiResult<List<BookingModel>>> GetClientBookingsAsync(Guid clientId);
    Task<ApiResult<List<BookingModel>>> GetStaffBookingsAsync(Guid staffId);
    Task<ApiResult<List<BookingModel>>> GetAllBookingsAsync();
    Task<ApiResult<Guid>>  CreateAsync(CreateBookingRequest request);
    Task<ApiResult<bool>>  CancelAsync(Guid id, Guid requesterId, string requesterType);
    Task<ApiResult<bool>>  ConfirmAsync(Guid id);
    Task<ApiResult<bool>>  CompleteAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class BookingService(
    HttpClient http,
    ILocalStorageService localStorage,
    ToastService toast) : IBookingService
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

    // ── Queries (no success toast — avoid noise on reads) ──────────────────

    public async Task<ApiResult<List<BookingModel>>> GetClientBookingsAsync(Guid clientId)
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync($"api/bookings/client/{clientId}");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load your bookings.");
            ShowErrors(errors);
            return ApiResult<List<BookingModel>>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookingModel>>>();
        return ApiResult<List<BookingModel>>.Ok(result?.Data ?? []);
    }

    public async Task<ApiResult<List<BookingModel>>> GetStaffBookingsAsync(Guid staffId)
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync($"api/bookings/staff/{staffId}");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load staff bookings.");
            ShowErrors(errors);
            return ApiResult<List<BookingModel>>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookingModel>>>();
        return ApiResult<List<BookingModel>>.Ok(result?.Data ?? []);
    }

    public async Task<ApiResult<List<BookingModel>>> GetAllBookingsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await http.GetAsync("api/bookings");
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to load bookings.");
            ShowErrors(errors);
            return ApiResult<List<BookingModel>>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BookingModel>>>();
        return ApiResult<List<BookingModel>>.Ok(result?.Data ?? []);
    }

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task<ApiResult<Guid>> CreateAsync(CreateBookingRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await http.PostAsJsonAsync("api/bookings", request);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to create booking.");
            ShowErrors(errors);
            return ApiResult<Guid>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Booking created successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<Guid>.Ok(result?.Data ?? Guid.Empty, msg);
    }

    public async Task<ApiResult<bool>> CancelAsync(Guid id, Guid requesterId, string requesterType)
    {
        await SetAuthHeaderAsync();
        var body = new { BookingId = id, RequesterId = requesterId, RequesterType = requesterType };
        var response = await http.PostAsJsonAsync($"api/bookings/{id}/cancel", body);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to cancel booking.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Booking cancelled successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }

    public async Task<ApiResult<bool>> ConfirmAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await http.PostAsync($"api/bookings/{id}/confirm", null);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to confirm booking.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Booking confirmed successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }

    public async Task<ApiResult<bool>> CompleteAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await http.PostAsync($"api/bookings/{id}/complete", null);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to complete booking.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Booking completed successfully.";
        toast.ShowSuccess(msg);
        return ApiResult<bool>.Ok(true, msg);
    }
}
