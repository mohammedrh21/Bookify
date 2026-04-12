using Bookify.Client.Models;
using System.Net.Http.Json;

namespace Bookify.Client.Services
{
    public interface IStaffApiService
    {
        Task<PagedResult<StaffClientModel>> GetStaffClientsAsync(string? search = null, DateTime? dateFilter = null, int page = 1, int pageSize = 10);
        Task<StaffClientDetailsModel?> GetStaffClientDetailsAsync(Guid clientId);
    }

    public class StaffApiService(HttpClient http, ToastService toast) : BaseApiService(http, toast), IStaffApiService
    {
        public async Task<PagedResult<StaffClientModel>> GetStaffClientsAsync(string? search = null, DateTime? dateFilter = null, int page = 1, int pageSize = 10)
        {
            var query = $"api/staff/clients?page={page}&pageSize={pageSize}";
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                query += $"&search={Uri.EscapeDataString(search)}";
            }
            if (dateFilter.HasValue)
            {
                query += $"&dateFilter={dateFilter.Value:yyyy-MM-dd}";
            }

            var result = await GetAsync<PagedResult<StaffClientModel>>(query, "Failed to load staff clients.");
            return result.Data ?? new PagedResult<StaffClientModel>();
        }

        public async Task<StaffClientDetailsModel?> GetStaffClientDetailsAsync(Guid clientId)
        {
            var result = await GetAsync<StaffClientDetailsModel>($"api/staff/clients/{clientId}/details", "Failed to load client details.");
            return result.Data;
        }
    }

    public class StaffClientModel
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int TotalBookings { get; set; }
        public DateTime? FirstBookingDate { get; set; }
        public DateTime? LastBookingDate { get; set; }
    }

    public class StaffClientDetailsModel
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public List<StaffClientBookingModel> Bookings { get; set; } = new();
    }

    public class StaffClientBookingModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
