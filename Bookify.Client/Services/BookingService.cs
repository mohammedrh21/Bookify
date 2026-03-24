using Bookify.Client.Models;
using Bookify.Client.Models.Booking;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IBookingService
{
    Task<ApiResult<List<BookingModel>>>         GetClientBookingsAsync(Guid clientId);
    Task<ApiResult<List<BookingModel>>>         GetStaffBookingsAsync(Guid staffId, int page = 1, int pageSize = 10);
    Task<ApiResult<PagedResult<BookingModel>>>  GetStaffBookingsPagedAsync(
        Guid staffId, int page = 1, int pageSize = 10,
        string? status = null, DateTime? from = null, DateTime? to = null,
        string? search = null, bool sortAsc = true);
    Task<ApiResult<PagedResult<BookingModel>>>  GetAllBookingsAsync(
        int page = 1, int pageSize = 10, string? status = null,
        string? search = null, string? staffName = null,
        Guid? categoryId = null, DateTime? from = null, DateTime? to = null);
    Task<ApiResult<Guid>>          CreateAsync(CreateBookingRequest request);
    Task<ApiResult<bool>>          CancelAsync(Guid id, Guid requesterId, string requesterType);
    Task<ApiResult<List<DateTime>>> GetOccupiedSlotsAsync(Guid serviceId, DateTime from, DateTime to);
    Task<ApiResult<bool>>          ConfirmAsync(Guid id);
    Task<ApiResult<bool>>          CompleteAsync(Guid id);
    Task<ApiResult<BookingModel?>> GetByIdAsync(Guid id);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class BookingService(HttpClient http, ToastService toast)
    : BaseApiService(http, toast), IBookingService
{
    // ── Queries ──────────────────────────────────────────────────────────

    public async Task<ApiResult<List<BookingModel>>> GetClientBookingsAsync(Guid clientId)
    {
        var result = await GetAsync<List<BookingModel>>($"api/bookings/client/{clientId}", "Failed to load your bookings.");
        return ApiResult<List<BookingModel>>.Ok(result.Data ?? []);
    }

    public async Task<ApiResult<List<BookingModel>>> GetStaffBookingsAsync(Guid staffId, int page = 1, int pageSize = 10)
    {
        var result = await GetAsync<List<BookingModel>>($"api/bookings/staff/{staffId}?page={page}&pageSize={pageSize}", "Failed to load staff bookings.");
        return ApiResult<List<BookingModel>>.Ok(result.Data ?? []);
    }

    public async Task<ApiResult<PagedResult<BookingModel>>> GetStaffBookingsPagedAsync(
        Guid staffId, int page = 1, int pageSize = 10,
        string? status = null, DateTime? from = null, DateTime? to = null,
        string? search = null, bool sortAsc = true)
    {
        var url = $"api/bookings/staff/{staffId}?page={page}&pageSize={pageSize}&sortAsc={sortAsc}";
        if (!string.IsNullOrWhiteSpace(status)) url += $"&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (from.HasValue) url += $"&from={from.Value:yyyy-MM-dd}";
        if (to.HasValue)   url += $"&to={to.Value:yyyy-MM-dd}";

        var result = await GetAsync<PagedResult<BookingModel>>(url, "Failed to load staff bookings.");
        return ApiResult<PagedResult<BookingModel>>.Ok(result.Data ?? new PagedResult<BookingModel>());
    }

    public async Task<ApiResult<PagedResult<BookingModel>>> GetAllBookingsAsync(
        int page = 1, int pageSize = 10,
        string? status     = null,
        string? search     = null,
        string? staffName  = null,
        Guid?   categoryId = null,
        DateTime? from     = null,
        DateTime? to       = null)
    {
        var url = $"api/bookings?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))    url += $"&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrWhiteSpace(search))    url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(staffName)) url += $"&staffName={Uri.EscapeDataString(staffName)}";
        if (categoryId.HasValue)                   url += $"&categoryId={categoryId.Value}";
        if (from.HasValue)                         url += $"&from={from.Value:yyyy-MM-dd}";
        if (to.HasValue)                           url += $"&to={to.Value:yyyy-MM-dd}";

        var result = await GetAsync<PagedResult<BookingModel>>(url, "Failed to load bookings.");
        return ApiResult<PagedResult<BookingModel>>.Ok(result.Data ?? new PagedResult<BookingModel>());
    }

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task<ApiResult<Guid>> CreateAsync(CreateBookingRequest request)
        => await PostAsync<CreateBookingRequest, Guid>("api/bookings", request, "Failed to create booking.");

    public async Task<ApiResult<List<DateTime>>> GetOccupiedSlotsAsync(Guid serviceId, DateTime from, DateTime to)
    {
        var url = $"api/bookings/service/{serviceId}/occupied-slots?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var result = await GetAsync<List<DateTime>>(url, "Failed to load occupied slots.");
        return ApiResult<List<DateTime>>.Ok(result.Data ?? []);
    }

    public async Task<ApiResult<bool>> CancelAsync(Guid id, Guid requesterId, string requesterType)
    {
        var body = new { BookingId = id, RequesterId = requesterId, RequesterType = requesterType };
        return await PostAsync($"api/bookings/{id}/cancel", body, "Failed to cancel booking.");
    }

    public async Task<ApiResult<bool>> ConfirmAsync(Guid id)
        => await PostAsync($"api/bookings/{id}/confirm", (object)null!, "Failed to confirm booking.");

    public async Task<ApiResult<bool>> CompleteAsync(Guid id)
        => await PostAsync($"api/bookings/{id}/complete", (object)null!, "Failed to complete booking.");

    public async Task<ApiResult<BookingModel?>> GetByIdAsync(Guid id)
        => await GetAsync<BookingModel?>($"api/bookings/{id}", "Booking not found.");
}

