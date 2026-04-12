using Bookify.Client.Models;
using Bookify.Client.Models.Common;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

public interface IStatisticsApiService
{
    /// <summary>Admin-scoped dashboard stats (platform-wide).</summary>
    Task<ApiResult<AdminDashboardResponse>> GetAdminDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>Staff-scoped dashboard stats (own bookings + reviews only).</summary>
    Task<ApiResult<StaffDashboardResponse>> GetStaffDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

public class StatisticsApiService(HttpClient http, ToastService toast)
    : BaseApiService(http, toast), IStatisticsApiService
{
    public async Task<ApiResult<AdminDashboardResponse>> GetAdminDashboardStatsAsync(
        DateTime? startDate = null, DateTime? endDate = null)
        => await FetchAsync<AdminDashboardResponse>("api/statistics/admin/dashboard", startDate, endDate);

    public async Task<ApiResult<StaffDashboardResponse>> GetStaffDashboardStatsAsync(
        DateTime? startDate = null, DateTime? endDate = null)
        => await FetchAsync<StaffDashboardResponse>("api/statistics/staff/dashboard", startDate, endDate);

    private async Task<ApiResult<T>> FetchAsync<T>(
        string baseUrl, DateTime? startDate, DateTime? endDate)
    {
        var url = baseUrl;
        var sep = '?';
        if (startDate.HasValue) { url += $"{sep}from={startDate.Value:yyyy-MM-dd}"; sep = '&'; }
        if (endDate.HasValue)   { url += $"{sep}to={endDate.Value:yyyy-MM-dd}";     sep = '&'; }

        return (await GetAsync<T>(url, "Failed to fetch statistics."))!;
    }
}

