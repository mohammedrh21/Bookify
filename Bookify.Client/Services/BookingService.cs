using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.Booking;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IBookingService
{
    Task<List<BookingModel>> GetClientBookingsAsync(Guid clientId);
    Task<List<BookingModel>> GetStaffBookingsAsync(Guid staffId);
    Task<List<BookingModel>> GetAllBookingsAsync();
    Task<BookingModel?> GetByIdAsync(Guid id);
    Task<Guid?> CreateAsync(CreateBookingRequest request);
    Task<bool> CancelAsync(Guid id, Guid requesterId, string requesterType);
    Task<bool> ConfirmAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class BookingService(HttpClient http, ILocalStorageService localStorage)
    : IBookingService
{
    private async Task SetAuthHeader()
    {
        var token = await localStorage.GetItemAsync<string>("access_token");
        if (!string.IsNullOrEmpty(token))
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<BookingModel>> GetClientBookingsAsync(Guid clientId)
    {
        await SetAuthHeader();
        var response = await http.GetFromJsonAsync<ApiResponse<List<BookingModel>>>(
            $"api/bookings/client/{clientId}");
        return response?.Data?.ToList() ?? [];
    }

    public async Task<List<BookingModel>> GetStaffBookingsAsync(Guid staffId)
    {
        await SetAuthHeader();
        var response = await http.GetFromJsonAsync<ApiResponse<List<BookingModel>>>(
            $"api/bookings/staff/{staffId}");
        return response?.Data?.ToList() ?? [];
    }

    public async Task<List<BookingModel>> GetAllBookingsAsync()
    {
        await SetAuthHeader();
        var response = await http.GetFromJsonAsync<ApiResponse<List<BookingModel>>>("api/bookings");
        return response?.Data?.ToList() ?? [];
    }

    public async Task<BookingModel?> GetByIdAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.GetFromJsonAsync<ApiResponse<BookingModel>>($"api/bookings/{id}");
        return response?.Data;
    }

    public async Task<Guid?> CreateAsync(CreateBookingRequest request)
    {
        await SetAuthHeader();
        var response = await http.PostAsJsonAsync("api/bookings", request);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        return result?.Data;
    }

    /// <summary>
    /// Cancels a booking via POST (matching the API's POST endpoint).
    /// </summary>
    public async Task<bool> CancelAsync(Guid id, Guid requesterId, string requesterType)
    {
        await SetAuthHeader();
        var body = new { BookingId = id, RequesterId = requesterId, RequesterType = requesterType };
        var response = await http.PostAsJsonAsync($"api/bookings/{id}/cancel", body);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Confirms a booking via POST (matching the API's POST endpoint).
    /// </summary>
    public async Task<bool> ConfirmAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.PostAsync($"api/bookings/{id}/confirm", null);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Completes a booking via POST (matching the API's POST endpoint).
    /// </summary>
    public async Task<bool> CompleteAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.PostAsync($"api/bookings/{id}/complete", null);
        return response.IsSuccessStatusCode;
    }
}
