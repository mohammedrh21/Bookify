using Bookify.Client.Models;
using System.Net.Http.Json;

namespace Bookify.Client.Services
{
    public interface IUserApiService
    {
        Task<PagedResult<StaffSummaryModel>>  GetStaffAsync(int page = 1, int pageSize = 10);
        Task<PagedResult<ClientSummaryModel>> GetClientsAsync(int page = 1, int pageSize = 10);
        Task<bool>                            ToggleUserActiveAsync(Guid userId);
        Task<ClientReportModel?>              GetClientReportAsync(Guid clientId);
    }

    public class UserApiService(HttpClient http, ToastService toast)
        : BaseApiService(http, toast), IUserApiService
    {
        public async Task<PagedResult<StaffSummaryModel>> GetStaffAsync(int page = 1, int pageSize = 10)
        {
            var result = await GetDirectAsync<PagedResult<StaffSummaryModel>>(
                $"api/users/staff?page={page}&pageSize={pageSize}", "Failed to load staff.");
            return result.Data ?? new PagedResult<StaffSummaryModel>();
        }

        public async Task<PagedResult<ClientSummaryModel>> GetClientsAsync(int page = 1, int pageSize = 10)
        {
            var result = await GetDirectAsync<PagedResult<ClientSummaryModel>>(
                $"api/users/clients?page={page}&pageSize={pageSize}", "Failed to load clients.");
            return result.Data ?? new PagedResult<ClientSummaryModel>();
        }

        public async Task<bool> ToggleUserActiveAsync(Guid userId)
        {
            var result = await PostAsync($"api/users/{userId}/toggle-active", (object)null!, "Failed to toggle user status.");
            return result.Success;
        }

        public async Task<ClientReportModel?> GetClientReportAsync(Guid clientId)
        {
            var result = await GetDirectAsync<ClientReportModel>($"api/users/clients/{clientId}/report", "Failed to load report.");
            return result.Data;
        }
    }


    // ── View Models (kept here to preserve existing references) ────────────────

    public class StaffSummaryModel
    {
        public Guid   Id            { get; set; }
        public string FullName      { get; set; } = default!;
        public string Phone         { get; set; } = default!;
        public bool   IsActive      { get; set; }
        public string ServiceName   { get; set; } = default!;
        public int    BookingCount  { get; set; }
        public string? ImagePath     { get; set; }
    }

    public class ClientSummaryModel
    {
        public Guid   Id           { get; set; }
        public string FullName     { get; set; } = default!;
        public string Phone        { get; set; } = default!;
        public bool   IsActive     { get; set; }
        public int    BookingCount { get; set; }
        public string? ImagePath    { get; set; }
    }

    public class ClientReportModel
    {
        public string              FullName       { get; set; } = default!;
        public string              Phone          { get; set; } = default!;
        public bool                IsActive       { get; set; }
        public int                 TotalBookings  { get; set; }
        public string?              ImagePath      { get; set; }
        public List<RecentBookingDto> RecentBookings { get; set; } = [];
    }

    public class RecentBookingDto
    {
        public Guid   Id          { get; set; }
        public string ServiceName { get; set; } = default!;
        public DateTime Date      { get; set; }
        public string Status      { get; set; } = default!;
    }
}
