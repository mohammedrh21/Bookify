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
        Task<PagedResult<AdminClientModel>>   GetAdminClientsAsync(string? search = null, int page = 1, int pageSize = 10);
        Task<AdminClientDetailsModel?>        GetAdminClientDetailsAsync(Guid clientId);

        // Admin Staff Members
        Task<PagedResult<AdminStaffModel>>    GetAdminStaffAsync(string? search = null, int page = 1, int pageSize = 10);
        Task<AdminStaffDetailsModel?>         GetAdminStaffDetailsAsync(Guid staffId);
    }

    public class UserApiService(HttpClient http, ToastService toast)
        : BaseApiService(http, toast), IUserApiService
    {
        public async Task<PagedResult<StaffSummaryModel>> GetStaffAsync(int page = 1, int pageSize = 10)
        {
            var result = await GetAsync<PagedResult<StaffSummaryModel>>(
                $"api/users/staff?page={page}&pageSize={pageSize}", "Failed to load staff.");
            return result.Data ?? new PagedResult<StaffSummaryModel>();
        }

        public async Task<PagedResult<ClientSummaryModel>> GetClientsAsync(int page = 1, int pageSize = 10)
        {
            var result = await GetAsync<PagedResult<ClientSummaryModel>>(
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
            var result = await GetAsync<ClientReportModel>($"api/users/clients/{clientId}/report", "Failed to load report.");
            return result.Data;
        }

        public async Task<PagedResult<AdminClientModel>> GetAdminClientsAsync(string? search = null, int page = 1, int pageSize = 10)
        {
            var query = $"api/users/admin-clients?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            var result = await GetAsync<PagedResult<AdminClientModel>>(query, "Failed to load admin clients.");
            return result.Data ?? new PagedResult<AdminClientModel>();
        }

        public async Task<AdminClientDetailsModel?> GetAdminClientDetailsAsync(Guid clientId)
        {
            var result = await GetAsync<AdminClientDetailsModel>($"api/users/clients/{clientId}/admin-details", "Failed to load client details.");
            return result.Data;
        }

        // ── Admin Staff Members ────────────────────────────────────────────────

        public async Task<PagedResult<AdminStaffModel>> GetAdminStaffAsync(string? search = null, int page = 1, int pageSize = 10)
        {
            var query = $"api/users/admin-staff?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            var result = await GetAsync<PagedResult<AdminStaffModel>>(query, "Failed to load staff members.");
            return result.Data ?? new PagedResult<AdminStaffModel>();
        }

        public async Task<AdminStaffDetailsModel?> GetAdminStaffDetailsAsync(Guid staffId)
        {
            var result = await GetAsync<AdminStaffDetailsModel>($"api/users/staff/{staffId}/admin-details", "Failed to load staff details.");
            return result.Data;
        }
    }


    // ── View Models ────────────────────────────────────────────────────────────

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

    public class AdminClientModel
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int TotalBookings { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminClientDetailsModel
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public List<AdminClientBookingModel> Bookings { get; set; } = new();
    }

    public class AdminClientBookingModel
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PaymentStatus { get; set; }
    }

    // ── Admin Staff Models ─────────────────────────────────────────────────────

    public class AdminStaffModel
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public Guid? ServiceId { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public double Rating { get; set; }
        public DateTime? JoinedDate { get; set; }
    }

    public class AdminStaffDetailsModel
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }

        public Guid? ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public List<AdminStaffBookingModel> Bookings { get; set; } = new();
    }

    public class AdminStaffBookingModel
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
