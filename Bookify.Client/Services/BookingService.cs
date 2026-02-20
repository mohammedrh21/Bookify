using Blazored.LocalStorage;
using Bookify.Client.Models.Booking;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IBookingService
{
    Task<List<BookingModel>> GetMyBookingsAsync();
    Task<List<BookingModel>> GetAllBookingsAsync();
    Task<BookingModel?> GetByIdAsync(Guid id);
    Task<Guid?> CreateAsync(CreateBookingRequest request);
    Task<bool> CancelAsync(Guid id);
    Task<bool> ApproveAsync(Guid id);
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

    public async Task<List<BookingModel>> GetMyBookingsAsync()
    {
        await SetAuthHeader();
        return await http.GetFromJsonAsync<List<BookingModel>>("api/bookings/my") ?? [];
    }

    public async Task<List<BookingModel>> GetAllBookingsAsync()
    {
        await SetAuthHeader();
        return await http.GetFromJsonAsync<List<BookingModel>>("api/bookings") ?? [];
    }

    public async Task<BookingModel?> GetByIdAsync(Guid id)
    {
        await SetAuthHeader();
        return await http.GetFromJsonAsync<BookingModel>($"api/bookings/{id}");
    }

    public async Task<Guid?> CreateAsync(CreateBookingRequest request)
    {
        await SetAuthHeader();
        var response = await http.PostAsJsonAsync("api/bookings", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task<bool> CancelAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.PatchAsync($"api/bookings/{id}/cancel", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.PatchAsync($"api/bookings/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        await SetAuthHeader();
        var response = await http.PatchAsync($"api/bookings/{id}/complete", null);
        return response.IsSuccessStatusCode;
    }
}
